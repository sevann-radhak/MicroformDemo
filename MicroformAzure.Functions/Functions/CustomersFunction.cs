using CyberSource.Api;
using CyberSource.Model;
using MicroformAzure.Functions.Entities;
using MicroformAzure.Functions.Helpers;
using MicroformAzure.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MicroformAzure.Functions.Functions
{
    public class CustomersFunction
    {
        public IConfiguration Configuration { get; }
        private readonly CyberSource.Client.Configuration _clientConfig;
        private readonly MicroformAzureContext _context;
        private string UrlFunctions { get; set; }

        public CustomersFunction(
            IConfiguration configuration,
            MicroformAzureContext context)
        {
            _clientConfig = MicroformHelper.SetUpCybersourceConfig(context);
            _context = context;
            Configuration = configuration;
            UrlFunctions = Configuration["UrlFunctions"];
        }

        /// <summary>
        /// Create a new Payment Instrument
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(nameof(CustomerPaymentInstrumentCreateAndAuthorizePaymentFunction))]
        public async Task<ActionResult<GenericResponse<PaymentResponseDTO>>> CustomerPaymentInstrumentCreateAndAuthorizePaymentFunction(
               [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "customers/{id}/payment-instruments")] HttpRequest req,
               string id,
               ILogger log)
        {
            log.LogInformation($"CustomerPaymentInstrumentCreate trigger function processed a request for {id} at {DateTime.UtcNow}.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            PaymentInstrumentCreateDTO dto = JsonConvert.DeserializeObject<PaymentInstrumentCreateDTO>(requestBody);

            if (dto is null)
            {
                return new BadRequestObjectResult("You need to send a valid body request.");
            }

            if (!dto.IsValid(validationResults: out ICollection<ValidationResult> validationResults))
            {
                return new BadRequestObjectResult(
                    $"Request is invalid: {string.Join(", ", validationResults.Select(s => s.ErrorMessage))}");
            }

            string customerTokenId = dto.CustomerId;
            bool _default = false;
            string cardExpirationMonth = dto.CardExpirationMonth;
            string cardExpirationYear = dto.CardExpirationYear;
            string cardType = "001";
            Tmsv2customersEmbeddedDefaultPaymentInstrumentCard card = new Tmsv2customersEmbeddedDefaultPaymentInstrumentCard(
                ExpirationMonth: cardExpirationMonth,
                ExpirationYear: cardExpirationYear,
                Type: cardType
           );

            string billToFirstName = dto.BillToFirstName;
            string billToLastName = dto.BillToLastName;
            string billToCompany = dto.BillToCompany;
            string billToAddress1 = dto.BillToAddress1;
            string billToLocality = dto.BillToLocality;
            string billToAdministrativeArea = dto.BillToAdministrativeArea;
            string billToPostalCode = dto.BillToPostalCode;
            string billToCountry = dto.BillToCountry;
            string billToEmail = dto.BillToEmail;
            string billToPhoneNumber = dto.BillToPhoneNumber;

            Tmsv2customersEmbeddedDefaultPaymentInstrumentBillTo billTo = new Tmsv2customersEmbeddedDefaultPaymentInstrumentBillTo(
                FirstName: billToFirstName,
                LastName: billToLastName,
                Company: billToCompany,
                Address1: billToAddress1,
                Locality: billToLocality,
                AdministrativeArea: billToAdministrativeArea,
                PostalCode: billToPostalCode,
                Country: billToCountry,
                Email: billToEmail,
                PhoneNumber: billToPhoneNumber
           );

            GenericResponse<Tmsv2customersEmbeddedDefaultPaymentInstrumentEmbeddedInstrumentIdentifier> instrumentIdentifierResponse =
                InstrumentIdentifierCreate(dto.CardNumber);

            if (!instrumentIdentifierResponse.IsSuccess)
            {
                return new ObjectResult(new GenericResponse<Tmsv2customersEmbeddedDefaultPaymentInstrument>
                {
                    Message = instrumentIdentifierResponse.Message
                });
            }

            string instrumentIdentifierId = instrumentIdentifierResponse.Result.Id;
            Tmsv2customersEmbeddedDefaultPaymentInstrumentInstrumentIdentifier instrumentIdentifier = new Tmsv2customersEmbeddedDefaultPaymentInstrumentInstrumentIdentifier(
                Id: instrumentIdentifierId
           );

            PostCustomerPaymentInstrumentRequest requestObj = new PostCustomerPaymentInstrumentRequest(
                _Default: _default,
                Card: card,
                BillTo: billTo,
                InstrumentIdentifier: instrumentIdentifier
           );

            try
            {
                CustomerPaymentInstrumentApi apiInstance = new CustomerPaymentInstrumentApi(_clientConfig);
                Tmsv2customersEmbeddedDefaultPaymentInstrument result = apiInstance.PostCustomerPaymentInstrument(customerTokenId, requestObj);
                log.LogInformation($"PaymentInstrumentCreate Result: {result} at {DateTime.UtcNow}");

                ApplicationPayerInfoEntity apie = await _context
                    .ApplicationPayerInfo
                    .Include(a => a.ApplicationRequests)
                    .Where(x => x.AccessCode == dto.AccessCode)
                    .FirstOrDefaultAsync();

                ApplicationPayerTokenEntity apte = new ApplicationPayerTokenEntity
                {
                    ApplicationPayerInfo = apie,
                    CardInfo = result.InstrumentIdentifier.Id,
                    CreatedTime = DateTime.UtcNow,
                    Token = result.Id
                };

                await _context.ApplicationPayerToken.AddAsync(apte);
                await _context.SaveChangesAsync();


                PaymentResponseDTO paymentResult = await AuthorizePaymentAsync(dto, result.Id, apte.ShippingAddressId);

                return new OkObjectResult(new GenericResponse<PaymentResponseDTO>
                {
                    IsSuccess = true,
                    Result = paymentResult
                });
            }
            catch (Exception e)
            {
                log.LogError($"Unexpected error: {e.Message} at {DateTime.UtcNow}");
                return new ObjectResult(new GenericResponse<Tmsv2customersEmbeddedDefaultPaymentInstrument>
                {
                    Message = e.Message
                });
            }
        }

        /// <summary>
        /// Retrive Customer information by Id
        /// </summary>
        /// <param name="req"></param>
        /// <param name="id"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(nameof(CustomersRetrieveFunction))]
        public ActionResult<CustomerModel_OLD> CustomersRetrieveFunction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers/{id}")] HttpRequest req,
            string id,
            ILogger log)
        {
            log.LogInformation($"CustomersRetrieve trigger function processed a request for {id} at {DateTime.UtcNow}");

            try
            {
                CustomerApi apiInstance = new CustomerApi(_clientConfig);
                TmsV2CustomersResponse result = apiInstance.GetCustomer(id);

                log.LogInformation($"CustomersRetrieve Result: {result} at {DateTime.UtcNow}");

                return new OkObjectResult(new GenericResponse<CustomerModel_OLD>
                {
                    IsSuccess = true,
                    Result = ToCustomerModel(result)
                });
            }
            catch (Exception e)
            {
                log.LogError($"Unexpected error: {e.Message} at {DateTime.UtcNow}");
                return new ObjectResult(new GenericResponse<CustomerModel_OLD>
                {
                    Message = e.Message
                });
            }
        }

        /// <summary>
        /// Retrieve Payment Instrument List by Customer Id
        /// </summary>
        /// <param name="req"></param>
        /// <param name="id"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(nameof(CustomersRetrievePaymentInstrumentsFunction))]
        public ActionResult<GenericResponse<ICollection<PaymentInstrumentModel>>> CustomersRetrievePaymentInstrumentsFunction(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers/{id}/payment-instruments")] HttpRequest req,
           string id,
           ILogger log)
        {
            log.LogInformation($"CustomersRetrievePaymentInstruments trigger function processed a request for {id} at {DateTime.UtcNow}");

            string profileid = null;
            long? offset = null;
            long? limit = null;

            try
            {
                CustomerPaymentInstrumentApi apiInstance = new CustomerPaymentInstrumentApi(_clientConfig);
                PaymentInstrumentList result = apiInstance.GetCustomerPaymentInstrumentsList(id, profileid, offset, limit);
                log.LogInformation($"PaymentInstrumentRetrieve Result: {result} at {DateTime.UtcNow}");

                return new OkObjectResult(new GenericResponse<ICollection<PaymentInstrumentModel>>
                {
                    IsSuccess = true,
                    Result = result.Embedded.PaymentInstruments.
                    Select(x => ToPaymentInstrumentModel(x))
                    .ToList()
                });
            }
            catch (Exception e)
            {
                log.LogError($"Unexpected error: {e.Message} at {DateTime.UtcNow}");
                return new ObjectResult(new GenericResponse<CustomerModel_OLD>
                {
                    Message = e.Message
                });
            }
        }

        private async Task<PaymentResponseDTO> AuthorizePaymentAsync(
            PaymentInstrumentCreateDTO dto,
            string paymentInstrumentIdentifier,
            string shippingAddressId)
        {
            AuthorizationWithCustomerIdAndPaymentInstrumentIdDTO request = new AuthorizationWithCustomerIdAndPaymentInstrumentIdDTO
            {
                AccessCode = dto.AccessCode,
                CustomerId = dto.CustomerId,
                PaymentInstrumentId = paymentInstrumentIdentifier,
                ShippingAddressId = shippingAddressId
            };

            StringContent content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            string url = $"{UrlFunctions}/api/AuthorizationWithCustomerIdAndPaymentInstrumentId";
            using HttpClient httpClient = new HttpClient();

            try
            {
                using HttpResponseMessage response = await httpClient.PostAsync(url, content);
                string responseStream = await response.Content.ReadAsStringAsync();
                GenericResponse<PaymentResponseDTO> result = JsonConvert
                    .DeserializeObject<GenericResponse<PaymentResponseDTO>>(responseStream);

                return result.Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return null;
            }
        }

        private GenericResponse<Tmsv2customersEmbeddedDefaultPaymentInstrumentEmbeddedInstrumentIdentifier> InstrumentIdentifierCreate(string cardNumber)
        {
            string profileid = "93B32398-AD51-4CC2-A682-EA3E93614EB1";

            Tmsv2customersEmbeddedDefaultPaymentInstrumentEmbeddedInstrumentIdentifierCard card = new
                Tmsv2customersEmbeddedDefaultPaymentInstrumentEmbeddedInstrumentIdentifierCard(Number: cardNumber);

            PostInstrumentIdentifierRequest requestObj = new PostInstrumentIdentifierRequest(Card: card);

            try
            {
                InstrumentIdentifierApi apiInstance = new InstrumentIdentifierApi(_clientConfig);
                Tmsv2customersEmbeddedDefaultPaymentInstrumentEmbeddedInstrumentIdentifier result = apiInstance.PostInstrumentIdentifier(requestObj, profileid);

                return new GenericResponse<Tmsv2customersEmbeddedDefaultPaymentInstrumentEmbeddedInstrumentIdentifier>
                {
                    IsSuccess = true,
                    Result = result
                };
            }
            catch (Exception e)
            {
                return new GenericResponse<Tmsv2customersEmbeddedDefaultPaymentInstrumentEmbeddedInstrumentIdentifier>
                {
                    Message = e.Message
                };
            }
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

        private CustomerModel_OLD ToCustomerModel(TmsV2CustomersResponse customer)
        {
            //ApplicationPayerInfoEntity applicationPayerInfoEntity = _context
            //    .ApplicationPayerInfo
            //    .Include(a => a.ApplicationPayerTokens)
            //    .Where(t => t.CustomerId == customer.Id)
            //    .FirstOrDefault();

            //List<PaymentInstrumentModel> paymentInstruments = applicationPayerInfoEntity
            //    .ApplicationPayerTokens
            //    .Select(x =>
            //    {
            //        Tmsv2customersEmbeddedDefaultPaymentInstrument instr =
            //        PaymentInstrumentRetrieve(x.Token).Result;
            //        return ToPaymentInstrumentModel(instr);
            //    })
            //    .ToList();

            //return new CustomerModel_OLD
            //{
            //    Id = customer.Id,
            //    ClientReferenceInformationCode = customer.ClientReferenceInformation.Code,
            //    Creator = customer.Metadata.Creator,
            //    PaymentInstruments = paymentInstruments,
            //    ShippingAddressId = applicationPayerInfoEntity.ShippingAddressId
            //};

            return null; //TODO
        }

        private PaymentInstrumentModel ToPaymentInstrumentModel(Tmsv2customersEmbeddedDefaultPaymentInstrument data)
        {
            string cardNumber = data.Embedded.InstrumentIdentifier.Card.Number
                .Remove(0, 6);
            cardNumber = $"XXXXXX{cardNumber}";

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
                    InstrumentIdentifierId = data.Embedded.InstrumentIdentifier.Id,
                    PaymentInstrumentId = data.Id
                },
                Id = data.Id,
                InstrumentIdentifier = new InstrumentIdentifierModel
                {
                    InstrumentIdentifierId = data.Embedded.InstrumentIdentifier.Id,
                    Number = cardNumber, 
                    State = data.Embedded.InstrumentIdentifier.State
                },
                State = data.State
            };
        }
    }
}
