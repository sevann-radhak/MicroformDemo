using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MicroformAzure.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MicroformAzure.Web.Pages
{
    public class PaymentsHistoryModel : PageModel
    {
        private readonly ILogger<PaymentsHistoryModel> _logger;
        public string ApplicationName { get; set; }
        public string PayerId { get; set; }
        public PaymentRequestsByPayerIdDTO PaymentRequests { get; set; }
        private IConfiguration Configuration { get; }
        private string UrlFunctions { get; set; }

        public PaymentsHistoryModel(
           IConfiguration configuration,
           ILogger<PaymentsHistoryModel> logger)
        {
            _logger = logger;
            Configuration = configuration;
            PaymentRequests = new PaymentRequestsByPayerIdDTO 
            { 
                Requests = new List<RequestsByPayerIdDTO>() 
            };
        }

        public async Task<IActionResult> OnGet(string applicationName, string payerId)
        {
            ApplicationName = applicationName;
            PayerId = payerId;

            UrlFunctions = Configuration["UrlFunctions"];
            string url = $"{UrlFunctions}/api/application-payer-info/{payerId}";

            using HttpClient httpClient = new HttpClient();

            try
            {
                using HttpResponseMessage response = await httpClient.GetAsync(url);
                string responseStream = await response.Content.ReadAsStringAsync();
                GenericResponse<PaymentRequestsByPayerIdDTO> result = JsonConvert
                    .DeserializeObject<GenericResponse<PaymentRequestsByPayerIdDTO>>(responseStream);

                if (result != null && result.IsSuccess)
                {
                    string serializedObject = JsonConvert.SerializeObject(result.Result);
                    PaymentRequests = result.Result;
                    return null;
                }
                else
                {
                    string message = $"Error Response: {result.Message}";
                    _logger.LogError($"{message} at {DateTime.UtcNow}");
                    return RedirectToPage("Error", message);
                }
            }
            catch (Exception e)
            {
                string message = $"Error: {e.Message}";
                _logger.LogError($"{message} at {DateTime.UtcNow}");
                return RedirectToPage("Error", message);
            }
        }
    }
}
