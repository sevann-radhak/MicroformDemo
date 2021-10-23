using CyberSource.Api;
using CyberSource.Model;
using MicroformAzure.Functions.Entities;
using MicroformAzure.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MicroformAzure.Functions.Functions
{
    public class PaymentInstrumentFunction
    {
        private CyberSource.Client.Configuration _clientConfig;
        private readonly MicroformAzureContext _context;

        public PaymentInstrumentFunction(MicroformAzureContext context)
        {
            _context = context;
            SetUpCybersourceConfig();
        }

        /// <summary>
        /// Delete an Instrument Identifier by using Cybersource SDK.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(nameof(PaymentInstrumentDeleteFunction))]
        public async Task<ActionResult<GenericResponse<string>>> PaymentInstrumentDeleteFunction(
               [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "paymentinstruments/{id}")] HttpRequest req,
               string id,
               ILogger log)
        {
            log.LogInformation($"PaymentInstrumentDelete trigger function processed a request for {id} at {DateTime.UtcNow}");
            string profileid = "93B32398-AD51-4CC2-A682-EA3E93614EB1";

            if (string.IsNullOrEmpty(id))
            {
                return new GenericResponse<string> {  Message = "Payment Instrument Id is required." };
            }

            ApplicationPayerTokenEntity apte = _context
                .ApplicationPayerToken
                .Where(x => x.PaymentInstrumentId == id)
                .FirstOrDefault();

            try
            {
                PaymentInstrumentApi apiInstance = new PaymentInstrumentApi(_clientConfig);
                apiInstance.DeletePaymentInstrument(id, profileid);
                log.LogInformation($"PaymentInstrumentDelete Result at {DateTime.UtcNow}");

                if(apte != null)
                {
                    _context.Remove(apte);
                    await _context.SaveChangesAsync();
                }

                return new OkObjectResult(new GenericResponse<string>
                {
                    IsSuccess = true,
                    Result = id
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
        /// Create a new Instrument Identifier by using Cybersource SDK.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(nameof(PaymentInstrumentRetrieveFunction))]
        public ActionResult<GenericResponse<Tmsv2customersEmbeddedDefaultPaymentInstrument>> PaymentInstrumentRetrieveFunction(
               [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "paymentinstruments/{id}")] HttpRequest req,
               string id,
               ILogger log)
        {
            log.LogInformation($"PaymentInstrumentRetrieve trigger function processed a request for {id} at {DateTime.UtcNow}");
            string profileid = "93B32398-AD51-4CC2-A682-EA3E93614EB1";

            try
            {
                PaymentInstrumentApi apiInstance = new PaymentInstrumentApi(_clientConfig);
                Tmsv2customersEmbeddedDefaultPaymentInstrument result = apiInstance.GetPaymentInstrument(id, profileid);
                log.LogInformation($"PaymentInstrumentRetrieve Result: {result} at {DateTime.UtcNow}");
                
                return new OkObjectResult(new GenericResponse<Tmsv2customersEmbeddedDefaultPaymentInstrument>
                {
                    IsSuccess = true,
                    Result = result
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

        [FunctionName(nameof(PaymentInstrumentUpdateFunction))]
        public async Task<ActionResult<GenericResponse<Tmsv2customersEmbeddedDefaultPaymentInstrument>>> PaymentInstrumentUpdateFunction(
               [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "paymentinstruments/{id}")] HttpRequest req,
               string id,
               ILogger log)
        {
            log.LogInformation($"PaymentInstrumentUpdate trigger function processed a request for {id} at {DateTime.UtcNow}.");

            string profileid = "93B32398-AD51-4CC2-A682-EA3E93614EB1";
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            PaymentInstrumentUpdateDTO dto = JsonConvert.DeserializeObject<PaymentInstrumentUpdateDTO>(requestBody);

            if (dto is null)
            {
                string message = $"You need to pass a valid body request.";
                log.LogError($"Request error: {message} at {DateTime.UtcNow}");
                return new BadRequestObjectResult(new GenericResponse<Tmsv2customersEmbeddedDefaultPaymentInstrument> 
                { 
                    Message = message
                });
            }

            if (!dto.IsValid(validationResults: out ICollection<ValidationResult> validationResults))
            {
                string message = $"Request is invalid: {string.Join(", ", validationResults.Select(s => s.ErrorMessage))}";
                log.LogError($"Request error: {message} at {DateTime.UtcNow}");
                return new BadRequestObjectResult(new GenericResponse<Tmsv2customersEmbeddedDefaultPaymentInstrument>
                {
                    Message = message
                });
            }

            string paymentInstrumentTokenId = dto.PaymentInstrumentId; 
            string cardExpirationMonth = dto.CardExpirationMonth; 
            string cardExpirationYear = dto.CardExpirationYear; 
            string cardType = "visa";
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

            string instrumentIdentifierId = dto.InstrumentIdentifierId;
            Tmsv2customersEmbeddedDefaultPaymentInstrumentInstrumentIdentifier instrumentIdentifier = new Tmsv2customersEmbeddedDefaultPaymentInstrumentInstrumentIdentifier(
                Id: instrumentIdentifierId
           );

            PatchPaymentInstrumentRequest requestObj = new PatchPaymentInstrumentRequest(
                Card: card,
                BillTo: billTo,
                InstrumentIdentifier: instrumentIdentifier
           );

            try
            {
                PaymentInstrumentApi apiInstance = new PaymentInstrumentApi(_clientConfig);
                Tmsv2customersEmbeddedDefaultPaymentInstrument result = apiInstance.PatchPaymentInstrument(paymentInstrumentTokenId, requestObj, profileid);
                log.LogInformation($"PatchPaymentInstrument Result: {result} at {DateTime.UtcNow}");
                return new OkObjectResult(new GenericResponse<Tmsv2customersEmbeddedDefaultPaymentInstrument>
                {
                    IsSuccess = true,
                    Result = result
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

        private void SetUpCybersourceConfig()
        {
            try
            {
                Dictionary<string, string> configDictionary = new Configuration(_context, "testrest")
                    .GetConfiguration();

                _clientConfig = new CyberSource.Client.Configuration(merchConfigDictObj: configDictionary);
            }
            catch (Exception e)
            {
                throw new Exception($"There was an error while configuring the API Instance. {e.Message}");
            }
        }
    }
}
