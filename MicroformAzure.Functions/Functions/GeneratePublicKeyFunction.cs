using CyberSource.Api;
using CyberSource.Model;
using MicroformAzure.Functions.Helpers;
using MicroformAzure.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MicroformAzure.Functions.Functions
{
    public class GeneratePublicKeyFunction
    {
        public IConfiguration Configuration { get; }
        private readonly CyberSource.Client.Configuration _clientConfig;
        private string UrlWeb { get; set; }

        public GeneratePublicKeyFunction(IConfiguration configuration, MicroformAzureContext context)
        {
            _clientConfig = MicroformHelper.SetUpCybersourceConfig(context);
            Configuration = configuration;
            UrlWeb = Configuration["UrlWeb"];

        }

        /// <summary>
        /// Generate a Flex Token Public Key
        /// </summary>
        /// <returns></returns>
        [FunctionName("GeneratePublicKeyFunction")]
        public ActionResult<GenericResponse<FlexV1KeysPost200Response>> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GeneratePublicKeyFunction")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string encryptionType = "RsaOaep";
            string format = "JWT";
            string targetOrigin = UrlWeb;

            GeneratePublicKeyRequest requestObj = new GeneratePublicKeyRequest(
                 EncryptionType: encryptionType,
                 TargetOrigin: targetOrigin
            );

            try
            {
                KeyGenerationApi apiInstance = new KeyGenerationApi(_clientConfig);
                FlexV1KeysPost200Response result = apiInstance.GeneratePublicKey(format, requestObj);

                return new GenericResponse<FlexV1KeysPost200Response>
                {
                    IsSuccess = true,
                    Result = result
                };
            }
            catch (Exception e)
            {
                string message = $"Error while creating Flex Public Key: {e.Message} {e.InnerException?.Message} {e.InnerException?.InnerException?.Message}";
                log.LogError(message);

                return new GenericResponse<FlexV1KeysPost200Response>
                {
                    IsSuccess = true,
                    Message = message
                };
            }
        }
    }
}
