using MicroformAzure.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MicroformAzure.Functions.Functions
{
    public class ApplicationSetupFunction
    {
        private readonly MicroformAzureContext _context;

        public ApplicationSetupFunction(
            MicroformAzureContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create a new record for Application Setup Entity
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(nameof(CreateApplicationSetupFunction))]
        public async Task<IActionResult> CreateApplicationSetupFunction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ApplicationSetup")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"CreateApplicationSetup trigger function processed a request at {DateTime.UtcNow}.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            ApplicationSetupEntity entity = JsonConvert.DeserializeObject<ApplicationSetupEntity>(requestBody);

            if (!entity.IsValid(validationResults: out System.Collections.Generic.ICollection<System.ComponentModel.DataAnnotations.ValidationResult> validationResults))
            {
                return new BadRequestObjectResult($"{nameof(ApplicationSetupEntity)} is invalid: {string.Join(", ", validationResults.Select(s => s.ErrorMessage))}");
            }

            entity.CreatedTime = DateTime.UtcNow;
            entity.IsActive = true;

            await _context.AddAsync(entity);
            _context.SaveChanges();

            string message = $"New Application Setup record stored on table at {DateTime.UtcNow}.";
            log.LogInformation(message);

            return new OkObjectResult(entity);
        }
    }
}
