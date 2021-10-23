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
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MicroformAzure.Functions.Functions
{
    public class FlexTokenFunction
    {
        private CyberSource.Client.Configuration _clientConfig;
        private readonly MicroformAzureContext _context;

        public FlexTokenFunction(MicroformAzureContext context)
        {
            _context = context;
            _clientConfig = MicroformHelper.SetUpCybersourceConfig(context);
        }

        /// <summary>
        /// Create a new Payment by using Transient token.
        /// It creates a new Customer, a new Payment instrument and Ship order
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(nameof(PaymentWithFlexTokenCreatePermanentTMSTokenFunction))]
        public async Task<ActionResult<GenericResponse<PaymentResponseDTO>>> PaymentWithFlexTokenCreatePermanentTMSTokenFunction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "PaymentWithFlexTokenCreatePermanentTMSToken")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"PaymentWithFlexTokenCreatePermanentTMSToken trigger function processed a request at {DateTime.UtcNow}.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            PaymentWithTransientTokenRequest dto = JsonConvert.DeserializeObject<PaymentWithTransientTokenRequest>(requestBody);

            ApplicationPayerInfoEntity apie = await _context
                    .ApplicationPayerInfo
                    .Include(a => a.ApplicationRequests)
                    .Where(x => x.AccessCode == dto.AccessCode)
                    .FirstOrDefaultAsync();

            ApplicationRequestEntity apre = apie
                .ApplicationRequests
                .Where(x => x.ReferenceId == dto.AccessCode)
                .FirstOrDefault();

            if (apie == null || apre == null)
            {
                string message = $"Error: Application payer request Info data not found.";
                log.LogError($"{message} at {DateTime.UtcNow}.");
                return new BadRequestObjectResult(message);
            }

            string clientReferenceInformationCode = apie.PayerId;
            Ptsv2paymentsClientReferenceInformation clientReferenceInformation = new Ptsv2paymentsClientReferenceInformation(
                Code: clientReferenceInformationCode
           );

            List<string> processingInformationActionList = new List<string>
            {
                "TOKEN_CREATE"
            };

            List<string> processingInformationActionTokenTypes = new List<string>
            {
                "customer",
                "paymentInstrument",
                "shippingAddress"
            };

            bool processingInformationCapture = false;

            Ptsv2paymentsProcessingInformation processingInformation = new Ptsv2paymentsProcessingInformation(
                ActionList: processingInformationActionList,
                ActionTokenTypes: processingInformationActionTokenTypes,
                Capture: processingInformationCapture
           );

            string orderInformationAmountDetailsTotalAmount = apre.Amount.ToString();
            string orderInformationAmountDetailsCurrency = apre.Currency;

            Ptsv2paymentsOrderInformationAmountDetails orderInformationAmountDetails = new Ptsv2paymentsOrderInformationAmountDetails(
                TotalAmount: orderInformationAmountDetailsTotalAmount,
                Currency: orderInformationAmountDetailsCurrency
           );

            string orderInformationShipToFirstName = dto.OrderInformationBillTo.FirstName;
            string orderInformationShipToLastName = dto.OrderInformationBillTo.LastName;
            string orderInformationShipToAddress1 = dto.OrderInformationBillTo.Address1;
            string orderInformationShipToLocality = dto.OrderInformationBillTo.Locality;
            string orderInformationShipToAdministrativeArea = dto.OrderInformationBillTo.AdministrativeArea;
            string orderInformationShipToPostalCode = dto.OrderInformationBillTo.PostalCode;
            string orderInformationShipToCountry = dto.OrderInformationBillTo.Country;

            Ptsv2paymentsOrderInformationShipTo orderInformationShipTo = new Ptsv2paymentsOrderInformationShipTo(
                FirstName: orderInformationShipToFirstName,
                LastName: orderInformationShipToLastName,
                Address1: orderInformationShipToAddress1,
                Locality: orderInformationShipToLocality,
                AdministrativeArea: orderInformationShipToAdministrativeArea,
                PostalCode: orderInformationShipToPostalCode,
                Country: orderInformationShipToCountry
           );

            Ptsv2paymentsOrderInformation orderInformation = new Ptsv2paymentsOrderInformation(
                AmountDetails: orderInformationAmountDetails,
                BillTo: dto.OrderInformationBillTo,
                ShipTo: orderInformationShipTo
           );

            Ptsv2paymentsTokenInformation tokenInformation = new Ptsv2paymentsTokenInformation(
                 TransientTokenJwt: dto.TokenInformation.TransientTokenJwt
            );

            CreatePaymentRequest requestObj = new CreatePaymentRequest(
                ClientReferenceInformation: clientReferenceInformation,
                ProcessingInformation: processingInformation,
                OrderInformation: orderInformation,
                TokenInformation: tokenInformation
           );

            try
            {
                using IDbContextTransaction transaction = _context.Database.BeginTransaction();

                PaymentRequestEntity pre = new PaymentRequestEntity
                {
                    ApplicationRequest = apre,
                    BillingInfo = JsonConvert.SerializeObject(dto.OrderInformationBillTo),
                    CreatedTime = DateTime.UtcNow
                };

                PaymentsApi apiInstance = new PaymentsApi(_clientConfig);
                PtsV2PaymentsPost201Response result = apiInstance.CreatePayment(requestObj);

                PaymentResultEntity prse = new PaymentResultEntity
                {
                    PaymentRequest = pre,
                    ReturnDesicion = result.Status,
                    ReturnResult = result.Id,
                    Status = result.Status,
                    CreatedTime = DateTime.UtcNow
                };

                await _context.PaymentRequest.AddAsync(pre);
                await _context.PaymentResult.AddAsync(prse);

                if (result.Status == "AUTHORIZED")
                {
                    string urlRedirectAfterPayment = MicroformHelper.GetUrlRedirectAfterPayment(result, apie, apre);

                    PaymentResponseDTO paymentResponse = new PaymentResponseDTO
                    {
                        AuthorizedAmount = result.OrderInformation.AmountDetails.AuthorizedAmount,
                        ApplicationName = apre.ApplicationName,
                        ClientReferenceCode = result.ClientReferenceInformation.Code,
                        Currency = apre.Currency,
                        CustomerId = result.TokenInformation.Customer.Id,
                        Id = result.Id,
                        InstrumentIdentifierId = result.TokenInformation.InstrumentIdentifier.Id,
                        PaymentInstrumentId = result.TokenInformation.PaymentInstrument.Id,
                        UrlRedirectAfterPayment = urlRedirectAfterPayment,
                        Status = result.Status
                    };

                    //_context.ApplicationPayerInfo.Attach(apie);
                    //apte.CustomerId = result.TokenInformation.Customer.Id;
                    //apte.ShippingAddressId = result.TokenInformation.ShippingAddress.Id;
                    //_context.Entry(apie).Property(x => x.CustomerId).IsModified = true;
                    //_context.Entry(apie).Property(x => x.ShippingAddressId).IsModified = true;

                    ApplicationPayerTokenEntity apte = new ApplicationPayerTokenEntity
                    {
                        ApplicationPayerInfo = apie,
                        CardInfo = result.TokenInformation.InstrumentIdentifier.Id,
                        CreatedTime = DateTime.UtcNow,
                        CustomerId = result.TokenInformation.Customer.Id,
                        InstrumentIdentifierId = result.TokenInformation.InstrumentIdentifier.Id,
                        PaymentInstrumentId = result.TokenInformation.PaymentInstrument.Id,
                        ShippingAddressId = result.TokenInformation.ShippingAddress.Id,
                        Token = result.TokenInformation.PaymentInstrument.Id
                    };

                    await _context.ApplicationPayerToken.AddAsync(apte);
                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    log.LogInformation($"Response: {result} at {DateTime.UtcNow}");

                    return new OkObjectResult(new GenericResponse<PaymentResponseDTO>
                    {
                        IsSuccess = true,
                        Result = paymentResponse
                    });
                }
                else
                {
                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    log.LogInformation($"Response: {result} at {DateTime.UtcNow}");

                    return new OkObjectResult(new GenericResponse<PaymentResponseDTO>
                    {
                        Message = result.Status
                    });
                }
            }
            catch (Exception e)
            {
                string message = $"Error, Exception on calling the API: {e.Message}";
                log.LogError($"{message} at {DateTime.UtcNow}");
                return new BadRequestObjectResult(new GenericResponse<PaymentResponseDTO>
                {
                    Message = message
                });
            }
        }
    }
}
