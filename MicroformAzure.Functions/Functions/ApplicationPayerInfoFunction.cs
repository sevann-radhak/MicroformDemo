using MicroformAzure.Functions.Entities;
using MicroformAzure.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
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
    public class ApplicationPayerInfoFunction
    {
        private readonly MicroformAzureContext _context;

        public ApplicationPayerInfoFunction(
            MicroformAzureContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create a new record for Application Payer Info Entity
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(nameof(ApplicationPayerInfoCreateFunction))]
        public async Task<IActionResult> ApplicationPayerInfoCreateFunction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "application-payer-info")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"ApplicationPayerInfoCreate trigger function processed a request at {DateTime.UtcNow}.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            ApplicationPayerInfoEntity entity = JsonConvert.DeserializeObject<ApplicationPayerInfoEntity>(requestBody);

            if (!entity.IsValid(validationResults: out ICollection<ValidationResult> validationResults))
            {
                return new BadRequestObjectResult($"Request is invalid: {string.Join(", ", validationResults.Select(s => s.ErrorMessage))}");
            }

            entity.CreatedTime = DateTime.UtcNow;

            await _context.AddAsync(entity);
            _context.SaveChanges();

            string message = $"New Application Payer Info stored on table at {DateTime.UtcNow}.";
            log.LogInformation(message);

            return new OkObjectResult(entity);
        }

        /// <summary>
        /// Retrieve Application Payer Info Entity by PayerId
        /// </summary>
        /// <param name="req"></param>
        /// <param name="id"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(nameof(ApplicationPayerInfoRetrieveByPayerIdFunction))]
        public ActionResult<GenericResponse<PaymentRequestsByPayerIdDTO>> ApplicationPayerInfoRetrieveByPayerIdFunction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "application-payer-info/{id}")] HttpRequest req,
            string id,
            ILogger log)
        {
            log.LogInformation($"ApplicationPayerInfoRetrieveByPayerId trigger function processed a request at {DateTime.UtcNow}.");

            try
            {
                ApplicationPayerInfoEntity apie = _context
                    .ApplicationPayerInfo
                    .Where(x => x.PayerId == id)
                    .Include(p => p.ApplicationRequests)
                    .ThenInclude(r => r.PaymentRequests)
                    .ThenInclude(s => s.PaymentResults)
                    .FirstOrDefault();

                if (apie == null)
                {
                    string message = $"Error: No data found for PayerId {id}";
                    log.LogError($"{message} at {DateTime.UtcNow}");
                    return new BadRequestObjectResult(new GenericResponse<PaymentRequestsByPayerIdDTO>
                    {
                        Message = message
                    });
                }

                PaymentRequestsByPayerIdDTO response = new PaymentRequestsByPayerIdDTO
                {
                    Requests = new List<RequestsByPayerIdDTO>()
                };

                apie.ApplicationRequests
                    .Select(request =>
                    {
                        request.PaymentRequests
                            .Select(x =>
                            {
                                response.AccessCode = request.ReferenceId;
                                //response.CustomerId = apie.ApplicationPayerTokens.FirstOrDefault().CustomerId; //TODO: verify
                                response.PayerId = apie.PayerId;
                                response.Requests.Add(new RequestsByPayerIdDTO
                                {
                                    ApplicationName = request.ApplicationName,
                                    Currency = request.Currency,
                                    OrderCode = request.OrderCode,
                                    RequestCreatedTime = x.CreatedTime,
                                    ResultCreatedTime = x.PaymentResults?            
                                    .FirstOrDefault()?
                                    .CreatedTime,
                                    ReturnDesicion = x.PaymentResults?
                                    .FirstOrDefault()?
                                    .ReturnDesicion,
                                    ReturnResult = x.PaymentResults?
                                    .FirstOrDefault()?
                                    .ReturnResult,
                                    TotalAmount = request.Amount.ToString()
                                });
                                return x;
                            })
                            .ToList();


                        return request;
                    })
                    .ToList();

                response.UrlRedirectAfterPayment = GetUrlRedirectAfterHistory(apie);

                return new OkObjectResult(new GenericResponse<PaymentRequestsByPayerIdDTO>
                {
                    IsSuccess = true,
                    Result = response
                });
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                log.LogError(e.InnerException?.Message);
                log.LogError(e.InnerException?.InnerException?.Message);

                return new ObjectResult(new GenericResponse<PaymentRequestsByPayerIdDTO> { Message = e.Message });
            }
        }

        private string GetUrlRedirectAfterHistory(ApplicationPayerInfoEntity apie)
        {
            string urlRedirect = apie
                .ApplicationRequests
                .LastOrDefault()?
                .UrlRedirectAfterPayment;

            // Build a List of the querystring parameters (this could optionally also have a .ToLookup(qs => qs.key, qs => qs.value) call)
            var querystringParams = new[] {
                //new { key = "applicationName", value = apie.ApplicationName },
                new { key = "userId", value = apie.PayerId },
                new { key = "status", value = "OK" }
            };

            // format each querystring parameter, and ensure its value is encoded
            IEnumerable<string> encodedQueryStringParams = querystringParams
                .Select(p => string.Format("{0}={1}", p.key, HttpUtility.UrlEncode(p.value)));

            // Construct a strongly-typed Uri, with the querystring parameters appended
            UriBuilder url = new UriBuilder(urlRedirect)
            {
                Query = string.Join("&", encodedQueryStringParams)
            };

            return url.Uri.ToString();
        }
    }
}
