using CyberSource.Model;
using MicroformAzure.Functions.Entities;
using MicroformAzure.Functions.Helpers;
using MicroformAzure.Functions.Interface;
using MicroformAzure.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace MicroformAzure.Functions.Functions
{
    public class PaymentFunction
    {
        public IConfiguration Configuration { get; }
        private readonly CyberSource.Client.Configuration _clientConfig;
        private readonly MicroformAzureContext _context;
        private readonly IJwtAuthenticationService _jwtAuthenticationManager;
        private string UrlFunctions { get; set; }
        private string UrlWeb { get; set; }
        private readonly IApplicationLogsService _BdLog;

        public PaymentFunction(
            IJwtAuthenticationService jwtAuthenticationManager,
            MicroformAzureContext context,
            IConfiguration configuration,
            IApplicationLogsService BdLog)
        {
            _BdLog = BdLog;
            _clientConfig = MicroformHelper.SetUpCybersourceConfig(context);
            _context = context;
            _jwtAuthenticationManager = jwtAuthenticationManager;
            Configuration = configuration;
            UrlFunctions = Configuration["UrlFunctions"];
            UrlWeb = Configuration["UrlWeb"];

        }

        /// <summary>
        /// Send a Payment Request from Application origin
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(nameof(PaymentRequestFunction))]
        public async Task<IActionResult> PaymentRequestFunction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "PaymentRequest")] HttpRequest req,
            ILogger log)
        {
            string accessCode = string.Empty;

            log.LogInformation($"PaymentRequest trigger function processed a request at {DateTime.UtcNow}.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            AccessPaymentRequestDTO dto = JsonConvert.DeserializeObject<AccessPaymentRequestDTO>(requestBody);

            if (dto is null)
            {
                return new BadRequestObjectResult("You need to send a valid body request.");
            }

            if (!dto.IsValid(validationResults: out ICollection<ValidationResult> validationResults))
            {
                return new BadRequestObjectResult(
                    $"Request is invalid: {string.Join(", ", validationResults.Select(s => s.ErrorMessage))}");
            }

            if (GetAndValidateActiveApplicationKey(dto))
            {
                accessCode = _jwtAuthenticationManager.Authenticate(dto.ApplicationName, dto.ApplicationAccessKey);

                if (string.IsNullOrEmpty(accessCode))
                {
                    string eMessage = $"Token creation failed";
                    log.LogError($"{eMessage} at {DateTime.UtcNow}");
                    throw new Exception(eMessage);
                }

                try
                {
                    ApplicationRequestEntity applicationRequestEntity;

                    using IDbContextTransaction transaction = _context.Database.BeginTransaction();

                    ApplicationPayerInfoEntity applicationPayerInfoEntity = await _context
                        .ApplicationPayerInfo
                        .FirstOrDefaultAsync(a => a.PayerId == dto.PayerId);

                    if (applicationPayerInfoEntity == null)
                    {
                        applicationPayerInfoEntity = SetValuesApplicationPayerInfo(dto, accessCode);
                        await _context.AddAsync(applicationPayerInfoEntity);
                    }
                    else
                    {
                        _context.ApplicationPayerInfo.Attach(applicationPayerInfoEntity);
                        applicationPayerInfoEntity.AccessCode = accessCode;
                        applicationPayerInfoEntity.IsAccessCodeAvailable = true;

                        _context.Entry(applicationPayerInfoEntity)
                            .Property(x => x.AccessCode).IsModified = true;

                        _context.Entry(applicationPayerInfoEntity)
                            .Property(x => x.IsAccessCodeAvailable).IsModified = true;
                    }

                    applicationRequestEntity = SetValuesApplicationRequest(
                        dto,
                        applicationPayerInfoEntity,
                        accessCode);

                    await _context.AddAsync(applicationRequestEntity);

                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    await _BdLog.Log(_context, "Process: PaymentRequestFunction. ApplicationPayerTokenExistingID: ", applicationPayerInfoEntity.Id);

                    string message = $"New Token created";
                    log.LogInformation(message);

                    // Build a List of the querystring parameters (this could optionally also have a .ToLookup(qs => qs.key, qs => qs.value) call)
                    var querystringParams = new[] {
                        new { key = "accessCode", value = accessCode }
                    };

                    // format each querystring parameter, and ensure its value is encoded
                    IEnumerable<string> encodedQueryStringParams = querystringParams
                        .Select(p => string.Format("{0}={1}", p.key, HttpUtility.UrlEncode(p.value)));

                    // Construct a strongly-typed Uri, with the querystring parameters appended
                    UriBuilder url = new UriBuilder(UrlWeb)
                    {
                        Query = string.Join("&", encodedQueryStringParams)
                    };

                    return new OkObjectResult(url.Uri.ToString());
                }
                catch (Exception e)
                {
                    log.LogError(e.Message);
                    log.LogError(e.InnerException?.Message);
                    log.LogError(e.InnerException?.InnerException?.Message);

                    return new ObjectResult(e.Message);
                }
            }

            return new ObjectResult($"No data found for Application Name and Application key");
        }

        /// <summary>
        /// Request Payment Data by using Access Code
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(nameof(RequestDataWithAccessCode))]
        public async Task<ActionResult<GenericResponse<RespopnseWithAccessCodeDTO>>> RequestDataWithAccessCode(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "RequestDataWithAccessCode")] HttpRequest req,
            ILogger log)
        {
            string token = string.Empty;

            log.LogInformation($"RequestDataWithAccessCode received at {DateTime.UtcNow}.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                string accessCode = JsonConvert.DeserializeObject<string>(requestBody);

                if (string.IsNullOrEmpty(accessCode))
                {
                    string eMessage = $"Error: You need to pass a valid Access Code";
                    log.LogInformation($"{eMessage} at {DateTime.UtcNow}");
                    return new BadRequestObjectResult(eMessage);
                }

                ApplicationPayerInfoEntity applicationPayerInfoEntity = _context
                    .ApplicationPayerInfo
                    .Include(a => a.ApplicationPayerTokens)
                    .Include(i => i.ApplicationRequests)
                    .Where(t => t.AccessCode == accessCode && t.IsAccessCodeAvailable)
                    .FirstOrDefault();

                ApplicationRequestEntity applicationRequestEntity = _context.ApplicationRequest
                    .Where(t => t.ReferenceId == accessCode)
                    .FirstOrDefault();

                if (applicationPayerInfoEntity is null)
                {
                    return new NotFoundObjectResult(accessCode);
                }

                _context.ApplicationPayerInfo.Attach(applicationPayerInfoEntity);
                applicationPayerInfoEntity.IsAccessCodeAvailable = false;
                _context
                    .Entry(applicationPayerInfoEntity)
                    .Property(x => x.IsAccessCodeAvailable).IsModified = true;

                await _context.SaveChangesAsync();
                //await _BdLog.Log(_context, "Process: RequestDataWithAccessCode. ApplicationPayerTokenID: ", applicationPayerTokenEntity.Id);

                List<PaymentInstrumentModel> paymentInstruments = applicationPayerInfoEntity
                    .ApplicationPayerTokens
                    .Select(t =>
                    {
                        Tmsv2customersEmbeddedDefaultPaymentInstrument instrumentIdentifier = PaymentInstrumentRetrieve(t.PaymentInstrumentId).Result;
                        return ToPaymentInstrumentModel(instrumentIdentifier, t);
                    })
                    .ToList();

                string message = $"RequestDataWithAccessCode {accessCode} finishied.";
                log.LogInformation(message);

                DateTime eedc = new DateTime();

                DateTime.TryParse(applicationRequestEntity.ExecutionExact.ToString(), out eedc);

                return new OkObjectResult(new GenericResponse<RespopnseWithAccessCodeDTO>
                {
                    IsSuccess = true,
                    Result = new RespopnseWithAccessCodeDTO
                    {
                        AccessCode = accessCode,
                        ApplicationName = applicationRequestEntity.ApplicationName,
                        ClientReferenceInformationCode = applicationPayerInfoEntity.PayerId,
                        PaymentInstruments = paymentInstruments,
                        FlexResponse = await GenerateKeyAsync(),
                        ExecutionExact = eedc
                    }
                });
            }
            catch (Exception e)
            {
                string message = $"Error: {e.Message} {e.InnerException?.Message} {e.InnerException?.InnerException?.Message}";
                log.LogError(message);

                return new ObjectResult(new GenericResponse<RespopnseWithAccessCodeDTO> { Message = message });
            }
        }

        // TODO: validate as a JWT bearer
        [FunctionName(nameof(ValidateAccessCodeFunction))]
        public ActionResult<RespopnseWithAccessCodeDTO> ValidateAccessCodeFunction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ValidateAccessCode/{accessCode}")] HttpRequest req,
            string accessCode,
            ILogger log)
        {
            log.LogInformation($"Validation of Access code.");

            if (string.IsNullOrEmpty(accessCode))
            {
                return new BadRequestObjectResult("Invalid Access Code.");
            }

            bool valid = _context
                .ApplicationPayerInfo
                .Any(p => p.AccessCode == accessCode && p.IsAccessCodeAvailable);

            if (valid)
            {
                return new OkObjectResult(accessCode);
            }

            return new UnauthorizedObjectResult("Invalid Access Code");
        }

        /// <summary>
        /// Generate a Flex Token Public Key
        /// </summary>
        /// <returns></returns>
        private async Task<FlexV1KeysPost200Response> GenerateKeyAsync()
        {
            string url = $"{UrlFunctions}/api/GeneratePublicKeyFunction";
            using HttpClient httpClient = new HttpClient();

            using HttpResponseMessage response = await httpClient.GetAsync(url);
            string responseStream = await response.Content.ReadAsStringAsync();
            GenericResponse<FlexV1KeysPost200Response> result = JsonConvert
                .DeserializeObject<GenericResponse<FlexV1KeysPost200Response>>(responseStream);

            return result.Result;
        }

        private bool GetAndValidateActiveApplicationKey(AccessPaymentRequestDTO dto)
        {
            ApplicationSetupEntity entity = _context.ApplicationSetup
                .FirstOrDefault(a =>
                a.ApplicationKey == dto.ApplicationAccessKey
                && a.ApplicationName == dto.ApplicationName
                && a.IsActive == true);

            return entity != null;
        }

        private async Task<Tmsv2customersEmbeddedDefaultPaymentInstrument> PaymentInstrumentRetrieve(string id)
        {
            string url = $"{UrlFunctions}/api/PaymentInstruments/{id}";
            using HttpClient httpClient = new HttpClient();

            try
            {
                using HttpResponseMessage response = await httpClient.GetAsync(url);
                string responseStream = await response.Content.ReadAsStringAsync();
                GenericResponse<Tmsv2customersEmbeddedDefaultPaymentInstrument> result = JsonConvert
                    .DeserializeObject<GenericResponse<Tmsv2customersEmbeddedDefaultPaymentInstrument>>(responseStream);

                return result.Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return null;
            }
        }

        private async Task<CustomerModel_OLD> CustomerRetrieveAsync(string id)
        {
            string url = $"{UrlFunctions}/api/customers/{id}";
            using HttpClient httpClient = new HttpClient();

            try
            {
                using HttpResponseMessage response = await httpClient.GetAsync(url);
                string responseStream = await response.Content.ReadAsStringAsync();
                GenericResponse<CustomerModel_OLD> result = JsonConvert
                    .DeserializeObject<GenericResponse<CustomerModel_OLD>>(responseStream);

                return result.Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return null;
            }
        }

        private static ApplicationPayerInfoEntity SetValuesApplicationPayerInfo(AccessPaymentRequestDTO dto, string accesCode)
        {
            return new ApplicationPayerInfoEntity
            {
                AccessCode = accesCode,
                IsAccessCodeAvailable = true,
                PayerId = dto.PayerId
            };
        }

        private ApplicationRequestEntity SetValuesApplicationRequest(
            AccessPaymentRequestDTO dto,
            ApplicationPayerInfoEntity applicationPayerInfoEntity,
            string accessCode)
        {

            DateTime eedc = new DateTime();
            DateTime.TryParse(dto.ExecutionExact.ToString(), out eedc);


            //var arq = new ApplicationRequestEntity();

            //try
            //{
            //    arq.Amount = dto.Amount;
            //    arq.ApplicationPayerInfo = applicationPayerInfoEntity;
            //    arq.Currency = dto.Currency;
            //    arq.Language = dto.Language;
            //    arq.Message = "EXAMPLE MESSAGE";
            //    arq.OfficeName = dto.PaymentOffice;
            //    arq.PaymentMethod = dto.PaymentMethodsWithFee;
            //    arq.ReferenceId = accessCode;
            //    arq.TransactionCode = "EXAMPLE TRANSACTION CODE";
            //    arq.UrlRedirectAfterPayment = dto.UrlRedirectAfterPayment ?? $"{UrlWeb}/SuccessPayment";
            //    arq.ExecutionExact = eedc;
            //}
            //catch (Exception ex)
            //{ 

            //}


            return new ApplicationRequestEntity
            {
                Amount = dto.Amount,
                ApplicationName = dto.ApplicationName,
                ApplicationPayerInfo = applicationPayerInfoEntity,
                Currency = dto.Currency,
                Language = dto.Language,
                Message = "EXAMPLE MESSAGE",
                OfficeName = dto.PaymentOffice,
                OrderCode = dto.OrderCode,
                PaymentMethod = dto.PaymentMethodsWithFee,
                ReferenceId = accessCode,
                TransactionCode = "EXAMPLE TRANSACTION CODE",
                UrlRedirectAfterPayment = dto.UrlRedirectAfterPayment ?? $"{UrlWeb}/SuccessPayment",
                ExecutionExact = eedc
            };
        }

        private PaymentInstrumentModel ToPaymentInstrumentModel(
            Tmsv2customersEmbeddedDefaultPaymentInstrument data,
            ApplicationPayerTokenEntity t)
        {
            return new PaymentInstrumentModel
            {
                PaymentInstrumentInfo = new PaymentInstrumentUpdateDTO
                {
                    BillToAddress1 = data.BillTo.Address1,
                    BillToAdministrativeArea = data.BillTo.AdministrativeArea,
                    BillToCompany = data.BillTo.Company,
                    BillToCountry = data.BillTo.Country,
                    BillToEmail = data.BillTo.Email,
                    BillToFirstName = data.BillTo.FirstName,
                    BillToLastName = data.BillTo.LastName,
                    BillToLocality = data.BillTo.Locality,
                    BillToPhoneNumber = data.BillTo.PhoneNumber,
                    BillToPostalCode = data.BillTo.PostalCode,
                    CardExpirationMonth = data.Card.ExpirationMonth,
                    CardExpirationYear = data.Card.ExpirationYear,
                    CustomerId = t.CustomerId,
                    InstrumentIdentifierId = data.Embedded.InstrumentIdentifier.Id,
                    PaymentInstrumentId = data.Id,
                    ShippingAddressId = t.ShippingAddressId
                },
                Id = data.Id,
                InstrumentIdentifier = new InstrumentIdentifierModel
                {
                    InstrumentIdentifierId = data.Embedded.InstrumentIdentifier.Id,
                    Number = MicroformHelper.MaskCardNumber(data.Embedded.InstrumentIdentifier.Card.Number),
                    State = data.Embedded.InstrumentIdentifier.State
                },
                State = data.State
            };
        }
    }
}