using MicroformAzure.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CyberSource.Model;


namespace MicroformAzure.Web.Pages
{
    public class IndexModel : PageModel
    {
        public string flexresponse { get; set; }
        private readonly ILogger<IndexModel> _logger;
        private IConfiguration Configuration { get; }
        public PaymentProcessModelDTO PaymentModel { get; set; }
        private string UrlFunctions { get; set; }
        public DateTime ExecutionExact { get; set; }

        public IndexModel(
            IConfiguration configuration,
            ILogger<IndexModel> logger)
        {
            _logger = logger;
            Configuration = configuration;
            PaymentModel = new PaymentProcessModelDTO
            {
                PaymentInstruments = new List<PaymentInstrumentModel>()
            };
        }

        public async Task<IActionResult> OnGet(string accessCode)
        {
            UrlFunctions = Configuration["UrlFunctions"];

            if (string.IsNullOrEmpty(accessCode))
            {
                _logger.LogError($"Error: Invalid Access Code at {DateTime.UtcNow}");
                return RedirectToPage("Error", "Invalid Access Code");
            }

            string url = $"{UrlFunctions}/api/ValidateAccessCode/{accessCode}";
            using HttpClient httpClient = new HttpClient();
            try
            {
                using HttpResponseMessage response = await httpClient.PostAsync(url, null);
                string result = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("Error", "Access Code not valid.");
                }
                else
                {
                    PaymentProcessModelDTO data = await RequestWithAccessCodeAsync(accessCode);

                    if (data == null)
                    {
                        string message = $"No data found for access code: {accessCode}";
                        throw new Exception(message);
                    }

                    flexresponse = data.FlexResponse.KeyId;
                    PaymentModel.AccessCode = data.AccessCode.ToString();
                    PaymentModel.ApplicationName = data.ApplicationName.ToString();
                    PaymentModel.ClientReferenceInformationCode = data.ClientReferenceInformationCode;
                    PaymentModel.FlexResponse = data.FlexResponse;
                    PaymentModel.PaymentInstruments = data.PaymentInstruments;
                    PaymentModel.ExecutionExact = data.ExecutionExact;
                }
            }
            catch (Exception ex)
            {
                string message = $"Error: {ex.Message} - {ex?.InnerException?.Message} - {ex?.InnerException?.InnerException?.Message}";
                _logger.LogError($"{message} at {DateTime.UtcNow}");
                return RedirectToPage("Error", pageHandler: ex.Message.ToString());
            }

            return null;
        }

        public async Task<IActionResult> OnPost()
        {
            StringValues firstName = Request.Form["firstName"];
            StringValues lastName = Request.Form["lastName"];
            StringValues email = Request.Form["email"];
            StringValues address = Request.Form["address"];
            StringValues administrativeArea = Request.Form["administrativeArea"];
            StringValues locality = Request.Form["locality"];
            StringValues postalCode = Request.Form["postalCode"];
            StringValues accessCode = Request.Form["accessCode"];
            StringValues applicationName = Request.Form["applicationName"];
            StringValues transientToken = Request.Form["flexResponse"];
            StringValues executionExact = Request.Form["ExecutionExact"];

            DateTime eedate;

            DateTime.TryParse(executionExact, out eedate);

            if (eedate > DateTime.UtcNow)
            {
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

                string myContent = await content.ReadAsStringAsync();
                var res = await ScheduledPaymentsAsync(myContent, accessCode, applicationName, eedate);

                PaymentResponseDTO payment = new PaymentResponseDTO();
                payment.UrlRedirectAfterPayment = res != null ? (res + "&applicationName=" + applicationName + "&status=Scheduled") : "";          
                payment.Status = "scheduled";
                
                string serializedObject = JsonConvert.SerializeObject(payment);
                
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
            return RedirectToPage("Token", new
            {
                accessCode,
                applicationName,
                transientToken,
                administrativeArea,
                address,
                email,
                firstName,
                lastName,
                locality,
                    postalCode,
                    
            });
        }


        }


        private async Task<string> ScheduledPaymentsAsync(string Param1, string Param2, string Param3, DateTime ExecutionExact)
        {
            UrlFunctions = Configuration["UrlFunctions"];
            string url = $"{UrlFunctions}/api/ScheduledWorkInsert";
            
            using HttpClient httpClient = new HttpClient();

            ScheduledPaymentsDTO spe = new ScheduledPaymentsDTO
            {
                Param1 = Param1,
                Param2 = Param2,
                Param3 = Param3,
                Frequency ="E",        
                ExecutionExact  = ExecutionExact
            };

            StringContent content = new StringContent(JsonConvert.SerializeObject(spe), Encoding.UTF8, "application/json");

            try
            {
                using HttpResponseMessage response = await httpClient.PostAsync(url, content);
                string responseStream = await response.Content.ReadAsStringAsync();

                return responseStream;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return null;
            }
        }

        public async Task<IActionResult> OnPostAuthorizationWithCustomerIdAndPaymentInstrumentId()
        {
            UrlFunctions = Configuration["UrlFunctions"];
            string url = $"{UrlFunctions}/api/AuthorizationWithCustomerIdAndPaymentInstrumentId";

            StringValues accessCode = Request.Form["accessCode"];
            StringValues applicationName = Request.Form["applicationName"];
            StringValues paymentInformation = Request.Form["paymentInformation"];

            List<string> paymentInfo = paymentInformation.ToString().Split(',').ToList();

            string customerId = paymentInfo[0];
            string paymentInstrumentId = paymentInfo[1];
            string shippingAddressId = paymentInfo[2];
            StringValues executionExact = Request.Form["ExecutionExact"];

            AuthorizationWithCustomerIdAndPaymentInstrumentIdDTO request = new AuthorizationWithCustomerIdAndPaymentInstrumentIdDTO
            {
                AccessCode = accessCode,
                CustomerId = customerId,
                PaymentInstrumentId = paymentInstrumentId,
                ShippingAddressId = shippingAddressId
            };            

            using HttpClient httpClient = new HttpClient();
            StringContent content = new StringContent(
                JsonConvert.SerializeObject(request),
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

                    DateTime dtee = new DateTime();
                    DateTime.TryParse(executionExact, out dtee);
                    if (dtee > DateTime.UtcNow)

                    {
                        return RedirectToPage("Result", new { handler = "Your payment was Scheduled, you can close this window" });
                    }
                    else
                    {
                        return RedirectToPage(
                           "SuccessResponseGeneric",
                           new
                           {
                               applicationName,
                               serializedObject,
                               isPayment = true
                           });
                    }
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

        private async Task<PaymentProcessModelDTO> RequestWithAccessCodeAsync(string accessCode)
        {
            string url = $"{UrlFunctions}/api/RequestDataWithAccessCode";
            using HttpClient httpClient = new HttpClient();
            StringContent content = new StringContent(JsonConvert.SerializeObject(accessCode), Encoding.UTF8, "application/json");

            try
            {
                using HttpResponseMessage response = await httpClient.PostAsync(url, content);
                string responseStream = await response.Content.ReadAsStringAsync();

                GenericResponse<PaymentProcessModelDTO> result = JsonConvert
                    .DeserializeObject<GenericResponse<PaymentProcessModelDTO>>(responseStream);

                return result.Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return null;
            }
        }
    }
}
