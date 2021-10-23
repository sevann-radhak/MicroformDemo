using CyberSource.Model;
using MicroformAzure.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MicroformAzure.Web.Pages
{
    [BindProperties]
    public class PaymentElectronicCheckDebitsModel : PageModel
    {
        private readonly ILogger<PaymentElectronicCheckDebitsModel> _logger;
        public string ApplicationName { get; set; }
        public PaymenteCheckDTO Payment { get; set; }
        private IConfiguration Configuration { get; }
        private string UrlFunctions { get; set; }

        public PaymentElectronicCheckDebitsModel(
           IConfiguration configuration,
           ILogger<PaymentElectronicCheckDebitsModel> logger)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public void OnGet(
            string accessCode,
            string applicationName)
        {
            ApplicationName = applicationName;
            Payment = new PaymenteCheckDTO { AccessCode = accessCode };
        }

        public async Task<IActionResult> OnPost()
        {
            UrlFunctions = Configuration["UrlFunctions"];
            string url = $"{UrlFunctions}/api/paymentecheck";
            StringValues applicationName = Request.Form["applicationName"];
            StringValues accessCode = Request.Form["accessCode"];

            using HttpClient httpClient = new HttpClient();
            StringContent content = new StringContent(
                JsonConvert.SerializeObject(Payment),
                Encoding.UTF8,
                "application/json");

            try
            {
                using HttpResponseMessage response = await httpClient.PostAsync(url, content);
                string responseStream = await response.Content.ReadAsStringAsync();
                GenericResponse<PaymentResponseDTO> result = JsonConvert
                    .DeserializeObject<GenericResponse<PaymentResponseDTO>>(responseStream);

                if (result.IsSuccess)
                {
                    string serializedObject = JsonConvert.SerializeObject(result.Result);
                    return RedirectToPage(
                       "SuccessResponseGeneric",
                       new
                       {
                           applicationName,
                           serializedObject,
                           isPayment = true
                       });
                }
                else
                {
                    string message = $"NOT AUTHORIZED Response: {result.Message}";
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
