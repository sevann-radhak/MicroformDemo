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
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MicroformAzure.Functions.Functions
{
    public class ElectronicCheckDebitsFunction
    {
        private readonly CyberSource.Client.Configuration _clientConfig;
        private readonly MicroformAzureContext _context;

        public ElectronicCheckDebitsFunction(MicroformAzureContext context)
        {
            _clientConfig = MicroformHelper.SetUpCybersourceConfig(context);
            _context = context;
        }

        /// <summary>
        /// Create a new payment by using Electronic Check Debits
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(nameof(PaymentElectronicCheckDebitsFunction))]
        public async Task<ActionResult<PaymentResponseDTO>> PaymentElectronicCheckDebitsFunction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "paymentecheck")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"ElectronicCheckDebits trigger function processed a request at {DateTime.UtcNow}.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            PaymenteCheckDTO dto = JsonConvert
                .DeserializeObject<PaymenteCheckDTO>(requestBody);

            if (dto is null)
            {
                return new BadRequestObjectResult("You need to send a valid body request.");
            }

            if (!dto.IsValid(validationResults: out ICollection<ValidationResult> validationResults))
            {
                return new BadRequestObjectResult(
                    $"Request is invalid: {string.Join(", ", validationResults.Select(s => s.ErrorMessage))}");
            }

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

            string paymentInformationBankAccountType = dto.BankAccountType; ;
            string paymentInformationBankAccountNumber = dto.BankAccountNumber;
            Ptsv2paymentsPaymentInformationBankAccount paymentInformationBankAccount = new Ptsv2paymentsPaymentInformationBankAccount(
                Type: paymentInformationBankAccountType,
                Number: paymentInformationBankAccountNumber
           );

            string paymentInformationBankRoutingNumber = dto.BankRoutingNumber;
            Ptsv2paymentsPaymentInformationBank paymentInformationBank = new Ptsv2paymentsPaymentInformationBank(
                Account: paymentInformationBankAccount,
                RoutingNumber: paymentInformationBankRoutingNumber
           );

            string paymentInformationPaymentTypeName = "CHECK";
            Ptsv2paymentsPaymentInformationPaymentType paymentInformationPaymentType = new Ptsv2paymentsPaymentInformationPaymentType(
                Name: paymentInformationPaymentTypeName
           );

            Ptsv2paymentsPaymentInformation paymentInformation = new Ptsv2paymentsPaymentInformation(
                Bank: paymentInformationBank,
                PaymentType: paymentInformationPaymentType
           );

            string orderInformationAmountDetailsTotalAmount = apre.Amount.ToString();
            string orderInformationAmountDetailsCurrency = apre.Currency;
            Ptsv2paymentsOrderInformationAmountDetails orderInformationAmountDetails = new Ptsv2paymentsOrderInformationAmountDetails(
                TotalAmount: orderInformationAmountDetailsTotalAmount,
                Currency: orderInformationAmountDetailsCurrency
           );

            string orderInformationBillToFirstName = dto.BillToFirstName;
            string orderInformationBillToLastName = dto.BillToLastName; ;
            string orderInformationBillToAddress1 = dto.BillToAddress1;
            string orderInformationBillToLocality = dto.BillToLocality;
            string orderInformationBillToAdministrativeArea = dto.BillToAdministrativeArea;
            string orderInformationBillToPostalCode = dto.BillToPostalCode;
            string orderInformationBillToCountry = dto.BillToCountry;
            string orderInformationBillToEmail = dto.BillToEmail;

            Ptsv2paymentsOrderInformationBillTo orderInformationBillTo = new Ptsv2paymentsOrderInformationBillTo(
                FirstName: orderInformationBillToFirstName,
                LastName: orderInformationBillToLastName,
                Address1: orderInformationBillToAddress1,
                Locality: orderInformationBillToLocality,
                AdministrativeArea: orderInformationBillToAdministrativeArea,
                PostalCode: orderInformationBillToPostalCode,
                Country: orderInformationBillToCountry,
                Email: orderInformationBillToEmail
           );

            Ptsv2paymentsOrderInformation orderInformation = new Ptsv2paymentsOrderInformation(
                AmountDetails: orderInformationAmountDetails,
                BillTo: orderInformationBillTo
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
                    BillingInfo = JsonConvert.SerializeObject(orderInformationBillTo),
                    CreatedTime = DateTime.UtcNow
                };

                PaymentsApi apiInstance = new PaymentsApi(_clientConfig);
                PtsV2PaymentsPost201Response result = apiInstance.CreatePayment(requestObj);
                log.LogInformation($"PaymentInstrumentCreate Result: {result} at {DateTime.UtcNow}");

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

                return new OkObjectResult(new GenericResponse<PaymentResponseDTO>
                {
                    IsSuccess = true,
                    Result = ToPaymentResponse(result, apie, apre)
                });
            }
            catch (Exception e)
            {
                log.LogError($"Unexpected error: {e.Message} at {DateTime.UtcNow}");
                return new ObjectResult(new GenericResponse<PaymentResponseDTO>
                {
                    Message = e.Message
                });
            }
        }

        private PaymentResponseDTO ToPaymentResponse(
            PtsV2PaymentsPost201Response r,
            ApplicationPayerInfoEntity apie,
            ApplicationRequestEntity apre)
        {
            if (apre.UrlRedirectAfterPayment.EndsWith("/"))
            {
                apre.UrlRedirectAfterPayment = apre.UrlRedirectAfterPayment.Remove(apre.UrlRedirectAfterPayment.Length - 1, 1);
            }

            string urlRedirectAfterPayment = MicroformHelper.GetUrlRedirectAfterPayment(r, apie, apre);

            return new PaymentResponseDTO
            {
                //ApplicationName = apie.ApplicationName,
                AuthorizedAmount = r.OrderInformation.AmountDetails.AuthorizedAmount,
                ClientReferenceCode = r.ClientReferenceInformation.Code,
                Currency = r.OrderInformation.AmountDetails.Currency,
                Id = r.Id,
                Status = r.Status,
                UrlRedirectAfterPayment = urlRedirectAfterPayment,
                TotalAmount = r.OrderInformation.AmountDetails.TotalAmount
            };
        }
    }
}
