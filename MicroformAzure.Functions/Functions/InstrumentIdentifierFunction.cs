using CyberSource.Api;
using CyberSource.Model;
using MicroformAzure.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MicroformAzure.Functions.Functions
{
    public class InstrumentIdentifierFunction
    {
        private CyberSource.Client.Configuration _clientConfig;
        private readonly MicroformAzureContext _context;

        public InstrumentIdentifierFunction(MicroformAzureContext context)
        {
            _context = context;
            SetUpCybersourceConfig();
        }

        /// <summary>
        /// Create a new Instrument Identifier by using Cybersource SDK.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(nameof(InstrumentIdentifierCreateFunction))]
        public async Task<ActionResult<GenericResponse<Tmsv2customersEmbeddedDefaultPaymentInstrumentEmbeddedInstrumentIdentifier>>> InstrumentIdentifierCreateFunction(
               [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "InstrumentIdentifiers")] HttpRequest req,
               ILogger log)
        {
            log.LogInformation($"InstrumentIdentifierCreate trigger function processed a request at {DateTime.UtcNow}.");

            string profileid = "93B32398-AD51-4CC2-A682-EA3E93614EB1";
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            Tmsv2customersEmbeddedDefaultPaymentInstrumentEmbeddedInstrumentIdentifierCard dto = JsonConvert
                .DeserializeObject<Tmsv2customersEmbeddedDefaultPaymentInstrumentEmbeddedInstrumentIdentifierCard>(requestBody);

            Tmsv2customersEmbeddedDefaultPaymentInstrumentEmbeddedInstrumentIdentifierCard card = new 
                Tmsv2customersEmbeddedDefaultPaymentInstrumentEmbeddedInstrumentIdentifierCard(Number: dto.Number);
            
            PostInstrumentIdentifierRequest requestObj = new PostInstrumentIdentifierRequest(Card: card);
            
            try
            {
                InstrumentIdentifierApi apiInstance = new InstrumentIdentifierApi(_clientConfig);
                Tmsv2customersEmbeddedDefaultPaymentInstrumentEmbeddedInstrumentIdentifier result = apiInstance.PostInstrumentIdentifier(requestObj, profileid);

                log.LogInformation($"InstrumentIdentifierCreate Result: {result} at {DateTime.UtcNow}");
                return new OkObjectResult(new GenericResponse<Tmsv2customersEmbeddedDefaultPaymentInstrumentEmbeddedInstrumentIdentifier>{
                    IsSuccess = true,
                    Result = result 
                });
            }
            catch (Exception e)
            {
                log.LogError($"Unexpected error: {e.Message} at {DateTime.UtcNow}");
                return new ObjectResult(new GenericResponse<Tmsv2customersEmbeddedDefaultPaymentInstrumentEmbeddedInstrumentIdentifier>
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
