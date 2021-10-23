using MicroformAzure.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MicroformAzure.Web.Pages
{
    public class PaymentInstrumentDeleteModel : PageModel
    {
        public string ApplicationName { get; set; }
        private readonly ILogger<PaymentInstrumentDeleteModel> _logger;
        private IConfiguration Configuration { get; }
        private string UrlFunctions { get; set; }

        public PaymentInstrumentDeleteModel(
            IConfiguration configuration,
            ILogger<PaymentInstrumentDeleteModel> logger)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public async Task<IActionResult> OnGet(string id, string applicationName)
        {
            ApplicationName = applicationName;
            UrlFunctions = Configuration["UrlFunctions"];
            string url = $"{UrlFunctions}/api/PaymentInstruments/{id}";

            using HttpClient httpClient = new HttpClient();

            try
            {
                using HttpResponseMessage response = await httpClient.DeleteAsync(url);
                string responseStream = await response.Content.ReadAsStringAsync();
                GenericResponse<string> result = JsonConvert
                    .DeserializeObject<GenericResponse<string>>(responseStream);

                if (result.IsSuccess)
                {
                    string serializedObject = JsonConvert.SerializeObject(result.Result);
                    return RedirectToPage(
                       "SuccessResponseGeneric",
                       new
                       {
                           applicationName,
                           serializedObject,
                           isPayment = false
                       });
                }
                else
                {
                    string message = $"ERROR: {result.Message}";
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
