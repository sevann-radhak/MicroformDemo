using MicroformAzure.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MicroformAzure.Web.Pages
{
    public class PaymentExistingMethodModel : PageModel
    {
        public PaymentProcessModelDTO PaymentModel { get; set; }
        private readonly ILogger<PaymentExistingMethodModel> _logger;
        private IConfiguration Configuration { get; }
        private string UrlFunctions { get; set; }

        public PaymentExistingMethodModel(
            IConfiguration configuration,
            ILogger<PaymentExistingMethodModel> logger)
        {
            _logger = logger;
            Configuration = configuration;
            PaymentModel = new PaymentProcessModelDTO 
            {
                //CustomerModel = new CustomerModel()
            };
        }

        /// <summary>
        /// Payment by using an existing method
        /// </summary>
        /// <param name="accessCode"></param>
        /// <param name="applicationName"></param>
        /// <param name="customerId"></param>
        public async Task OnGet(
            string accessCode,
            string applicationName,
            string customerId)
        {
            UrlFunctions = Configuration["UrlFunctions"];
            PaymentModel.AccessCode = accessCode;
            PaymentModel.ApplicationName = applicationName;
            //PaymentModel.CustomerModel = await CustomerRetrieveAsync(customerId);
        }

        public async Task<IActionResult> OnPost()
        {
            //UrlFunctions = Configuration["UrlFunctions"];
            //string url = $"{UrlFunctions}/api/AuthorizationWithCustomerIdAndPaymentInstrumentId";

            //StringValues accessCode = Request.Form["accessCode"];
            //StringValues applicationName = Request.Form["applicationName"];
            //StringValues customerId = Request.Form["customerId"];
            //StringValues paymentInstrumentId = Request.Form["paymentInstrumentId"];
            //StringValues shippingAddressId = Request.Form["shippingAddressId"];

            //AuthorizationWithCustomerIdAndPaymentInstrumentIdDTO request = new AuthorizationWithCustomerIdAndPaymentInstrumentIdDTO
            //{
            //    AccessCode = accessCode,
            //    CustomerId = customerId,
            //    PaymentInstrumentId = paymentInstrumentId,
            //    ShippingAddressId = shippingAddressId
            //};

            //using HttpClient httpClient = new HttpClient();
            //StringContent content = new StringContent(
            //    JsonConvert.SerializeObject(request),
            //    Encoding.UTF8,
            //    "application/json");

            //try
            //{
            //    using HttpResponseMessage response = await httpClient.PostAsync(url, content);
            //    string responseStream = await response.Content.ReadAsStringAsync();
            //    GenericResponse<PaymentResponse> result = JsonConvert
            //        .DeserializeObject<GenericResponse<PaymentResponse>>(responseStream);

            //    if (result.IsSuccess)
            //    {
            //        string serializedObject = JsonConvert.SerializeObject(result.Result);
            //        return RedirectToPage(
            //           "SuccessResponseGeneric",
            //           new
            //           {
            //               applicationName,
            //               serializedObject,
            //               isPayment = true
            //           });
            //    }
            //    else
            //    {
            //        string message = $"NOT AUTHORIZED Response: {result.Message}";
            //        _logger.LogError($"{message} at {DateTime.UtcNow}");
            //        return RedirectToPage("Error", message);
            //    }
            //}
            //catch (Exception e)
            //{
            //    string message = $"Error: {e.Message}";
            //    _logger.LogError($"{message} at {DateTime.UtcNow}");
            //    return RedirectToPage("Error", message);
            //}

            return null;
        }

        private async Task<CustomerModel> CustomerRetrieveAsync(string id)
        {
            string url = $"{UrlFunctions}/api/customers/{id}";
            using HttpClient httpClient = new HttpClient();

            try
            {
                using HttpResponseMessage response = await httpClient.GetAsync(url);
                string responseStream = await response.Content.ReadAsStringAsync();
                GenericResponse<CustomerModel> result = JsonConvert
                    .DeserializeObject<GenericResponse<CustomerModel>>(responseStream);

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
