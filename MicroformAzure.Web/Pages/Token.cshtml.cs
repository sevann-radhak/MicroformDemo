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
    public class TokenModel : PageModel
    {
        public string AccessCode { get; set; }
        public string ApplicationName { get; set; }
        public string AdministrativeArea { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Locality { get; set; }
        public string PaymentResponse { get; set; }
        public string PostalCode { get; set; }
        public string TransientToken { get; set; }
        private readonly ILogger<TokenModel> _logger;
        private IConfiguration Configuration { get; }
        private string UrlFunctions { get; set; }

        public TokenModel(
            IConfiguration configuration, 
            ILogger<TokenModel> logger)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public void OnGet(
           string accessCode,
           string applicationName,
           string transientToken,
           string administrativeArea,
           string address,
           string email,
           string firstName,
           string lastName,
           string locality,
           string postalCode)
        {
            AccessCode = accessCode;
            Address = address;
            AdministrativeArea = administrativeArea;
            ApplicationName = applicationName;
            TransientToken = transientToken;
            AdministrativeArea = administrativeArea;
            Email = email;
            FirstName =firstName;
            LastName = lastName;
            Locality = locality;
            PostalCode = postalCode;
        }

        public async Task<IActionResult> OnPost()
        {
            UrlFunctions = Configuration["UrlFunctions"];
            string url = $"{UrlFunctions}/api/PaymentWithFlexTokenCreatePermanentTMSToken";

            StringValues firstName = Request.Form["firstName"];
            StringValues lastName = Request.Form["lastName"];
            StringValues email = Request.Form["email"];
            StringValues address = Request.Form["address"];
            StringValues administrativeArea = Request.Form["administrativeArea"];
            StringValues locality = Request.Form["locality"];
            StringValues postalCode = Request.Form["postalCode"];
            StringValues accessCode = Request.Form["accessCode"];
            StringValues transientToken = Request.Form["flexResponse"];
            StringValues applicationName = Request.Form["applicationName"];

            PaymentWithTransientTokenRequest request = new PaymentWithTransientTokenRequest
            {
                AccessCode = accessCode,
                OrderInformationBillTo = new Ptsv2paymentsOrderInformationBillTo
                {
                    Address1 = address,
                    AdministrativeArea = administrativeArea,
                    Country = "US",
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Locality = locality,
                    PhoneNumber = "4158880000",
                    PostalCode = postalCode
                },
                TokenInformation = new Ptsv2paymentsTokenInformation
                {
                    TransientTokenJwt = transientToken
                }
            };

            using HttpClient httpClient = new HttpClient();
            StringContent content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            try
            {
                using HttpResponseMessage response = await httpClient.PostAsync(url, content);
                string responseStream = await response.Content.ReadAsStringAsync();
                GenericResponse<PaymentResponseDTO> result = JsonConvert
                    .DeserializeObject<GenericResponse<PaymentResponseDTO>>(responseStream);

                if(result.IsSuccess)
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
                    string message = $"NOT AUTHORIZED response: {result.Message}";
                    _logger.LogError($"{message} at {DateTime.UtcNow}");
                    return RedirectToPage("Error", message);
                }                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                throw new Exception(message: ex.Message.ToString());
            }
        }
    }
}
