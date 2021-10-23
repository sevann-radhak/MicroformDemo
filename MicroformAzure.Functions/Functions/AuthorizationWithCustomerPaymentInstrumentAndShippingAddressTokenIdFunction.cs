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
    public class AuthorizationWithCustomerPaymentInstrumentAndShippingAddressTokenIdFunction
    {
        private CyberSource.Client.Configuration _clientConfig;
        private readonly MicroformAzureContext _context;

        public AuthorizationWithCustomerPaymentInstrumentAndShippingAddressTokenIdFunction(MicroformAzureContext context)
        {
            _context = context;
            _clientConfig = MicroformHelper.SetUpCybersourceConfig(context);
        }

        /// <summary>
        /// Authorize a payment by using CustomerId, PaymentInstrumentId and ShippingAddresId
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(nameof(AuthorizationWithCustomerIdAndPaymentInstrumentIdFunction))]
        public async Task<ActionResult<PaymentResponseDTO>> AuthorizationWithCustomerIdAndPaymentInstrumentIdFunction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "AuthorizationWithCustomerIdAndPaymentInstrumentId")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"AuthorizationWithCustomerIdAndPaymentInstrumentId trigger function processed a request at {DateTime.UtcNow}.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            AuthorizationWithCustomerIdAndPaymentInstrumentIdDTO dto = JsonConvert
                .DeserializeObject<AuthorizationWithCustomerIdAndPaymentInstrumentIdDTO>(requestBody);

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
                string message = $"Error: Application payer request Info data not found";
                log.LogError($"{message} at {DateTime.UtcNow}");

                return new BadRequestObjectResult(new GenericResponse<PtsV2PaymentsPost201Response> 
                {
                    Message = message
                });
            }

            string clientReferenceInformationCode = apie.PayerId;
            Ptsv2paymentsClientReferenceInformation clientReferenceInformation = new Ptsv2paymentsClientReferenceInformation(
                Code: clientReferenceInformationCode
           );

            string paymentInformationCustomerId = dto.CustomerId;
            Ptsv2paymentsPaymentInformationCustomer paymentInformationCustomer = new Ptsv2paymentsPaymentInformationCustomer(
                Id: paymentInformationCustomerId
           );

            string paymentInformationPaymentInstrumentId = dto.PaymentInstrumentId; 
            Ptsv2paymentsPaymentInformationPaymentInstrument paymentInformationPaymentInstrument = new Ptsv2paymentsPaymentInformationPaymentInstrument(
                Id: paymentInformationPaymentInstrumentId
           );

            string paymentInformationShippingAddressId = dto.ShippingAddressId; 
            Ptsv2paymentsPaymentInformationShippingAddress paymentInformationShippingAddress = new Ptsv2paymentsPaymentInformationShippingAddress(
                Id: paymentInformationShippingAddressId
           );

            Ptsv2paymentsPaymentInformation paymentInformation = new Ptsv2paymentsPaymentInformation(
                Customer: paymentInformationCustomer,
                PaymentInstrument: paymentInformationPaymentInstrument,
                ShippingAddress: paymentInformationShippingAddress
           );

            string orderInformationAmountDetailsTotalAmount = apre.Amount.ToString();
            string orderInformationAmountDetailsCurrency = apre.Currency;
            Ptsv2paymentsOrderInformationAmountDetails orderInformationAmountDetails = new Ptsv2paymentsOrderInformationAmountDetails(
                TotalAmount: orderInformationAmountDetailsTotalAmount,
                Currency: orderInformationAmountDetailsCurrency
           );

            Ptsv2paymentsOrderInformation orderInformation = new Ptsv2paymentsOrderInformation(
                AmountDetails: orderInformationAmountDetails
           );

            CreatePaymentRequest requestObj = new CreatePaymentRequest(
                ClientReferenceInformation: clientReferenceInformation,
                PaymentInformation: paymentInformation,
                OrderInformation: orderInformation
           );

            try
            {
                using IDbContextTransaction transaction = _context.Database.BeginTransaction();

                PaymentRequestEntity pre = new PaymentRequestEntity
                {
                    ApplicationRequest = apre,
                    BillingInfo = JsonConvert.SerializeObject("TODO: BILLING INFORMATION EXAMPLE."),
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
                await _context.SaveChangesAsync();
                transaction.Commit();

                if (result.Status == "AUTHORIZED")
                {
                    string urlRedirectAfterPayment = MicroformHelper.GetUrlRedirectAfterPayment(result, apie, apre);

                    PaymentResponseDTO paymentResponse = new PaymentResponseDTO
                    {
                        AuthorizedAmount = result.OrderInformation.AmountDetails.AuthorizedAmount,
                        ClientReferenceCode = result.ClientReferenceInformation.Code,
                        CustomerId = result.PaymentInformation.Customer.Id,
                        Currency = result.OrderInformation.AmountDetails.Currency,
                        Id = result.Id,
                        InstrumentIdentifierId = result.PaymentInformation.InstrumentIdentifier.Id,
                        PaymentInstrumentId = result.PaymentInformation.PaymentInstrument.Id,
                        Status = result.Status,
                        TotalAmount = result.OrderInformation.AmountDetails.TotalAmount,
                        UrlRedirectAfterPayment = urlRedirectAfterPayment
                    };

                    string message = $"AuthorizationWithCustomerIdAndPaymentInstrumentId finished: {result}";
                    log.LogInformation($"{message} at {DateTime.UtcNow}");

                    return new OkObjectResult(new GenericResponse<PaymentResponseDTO>
                    {
                        IsSuccess = true,
                        Result = paymentResponse
                    });
                }
                else
                {
                    return new ObjectResult(new GenericResponse<PaymentResponseDTO>
                    {
                        IsSuccess = false,
                        Message = result.Status
                    });
                }                
            }
            catch (Exception e)
            {
                string message = $"Exception on calling the API : {e.Message}";
                log.LogError($"{message} at {DateTime.UtcNow}");

                return new ObjectResult(new GenericResponse<PtsV2PaymentsPost201Response>
                {
                    IsSuccess = false,
                    Message = message
                });
            }
        }
    }
}
