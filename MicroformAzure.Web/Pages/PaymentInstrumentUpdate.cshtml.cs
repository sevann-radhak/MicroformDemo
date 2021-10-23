using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CyberSource.Model;
using MicroformAzure.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace MicroformAzure.Web.Pages
{
    public class PaymentInstrumentUpdateModel : PageModel
    {
        public string AccessCode { get; set; }
        public string ApplicationName { get; set; }
        public string CustomerId { get; set; }
        public PaymentInstrumentUpdateDTO PaymInstr { get; set; }
        private readonly ILogger<PaymentInstrumentUpdateModel> _logger;
        private IConfiguration Configuration { get; }
        private string UrlFunctions { get; set; }

        public PaymentInstrumentUpdateModel(
            IConfiguration configuration, 
            ILogger<PaymentInstrumentUpdateModel> logger)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public async Task<IActionResult> OnGet(
            string paymentInstrumentId,
            string instrumentIdentifierId,
            string applicationName,
            string customerId,
            string accessCode)
        {
            UrlFunctions = Configuration["UrlFunctions"];
            string url = $"{UrlFunctions}/api/PaymentInstruments/{paymentInstrumentId}";
            using HttpClient httpClient = new HttpClient();
            AccessCode = accessCode;
            ApplicationName = applicationName;
            CustomerId = customerId;
            PaymInstr = new PaymentInstrumentUpdateDTO();

            try
            {
                using HttpResponseMessage response = await httpClient.GetAsync(url);
                string responseStream = await response.Content.ReadAsStringAsync();
                GenericResponse<Tmsv2customersEmbeddedDefaultPaymentInstrument> result = 
                    JsonConvert.DeserializeObject<GenericResponse<Tmsv2customersEmbeddedDefaultPaymentInstrument>>(responseStream);

                if(result.IsSuccess)
                {
                    if (result.Result == null)
                    {
                        return RedirectToPage("Error", "No data found.");
                    }

                    PaymInstr = ToPaymentInstrumentUpdateDTO(result.Result, instrumentIdentifierId, paymentInstrumentId);

                    return null;
                }
                else
                {
                    return RedirectToPage("Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                string message = $"Error: {ex.Message}";
                _logger.LogError($"{message} at {DateTime.UtcNow}");
                return RedirectToPage("Error", message);
            }
        }

        public async Task<IActionResult> OnPost()
        {
            UrlFunctions = Configuration["UrlFunctions"];

            StringValues accessCode = Request.Form["accessCode"];
            StringValues customerId = Request.Form["customerId"];
            StringValues applicationName = Request.Form["applicationName"];
            StringValues billToAddress1 = Request.Form["billToAddress1"];
            StringValues billToAdministrativeArea = Request.Form["billToAdministrativeArea"];
            StringValues billToCompany = Request.Form["billToCompany"];
            StringValues billToCountry = Request.Form["billToCountry"]; 
            StringValues billToFirstName = Request.Form["billToFirstName"];
            StringValues billToEmail = Request.Form["billToEmail"];
            StringValues billToLastName = Request.Form["billToLastName"];
            StringValues billToLocality = Request.Form["billToLocality"];
            StringValues billToPhoneNumber = Request.Form["billToPhoneNumber"];
            StringValues billToPostalCode = Request.Form["billToPostalCode"];
            StringValues cardExpirationMonth = Request.Form["cardExpirationMonth"];
            StringValues cardExpirationYear = Request.Form["cardExpirationYear"];
            StringValues instrumentIdentifierId = Request.Form["instrumentIdentifierId"];
            StringValues paymentInstrumentTokenId = Request.Form["paymentInstrumentTokenId"];

            string url = $"{UrlFunctions}/api/PaymentInstruments/{paymentInstrumentTokenId}";

            PaymentInstrumentUpdateDTO request = new PaymentInstrumentUpdateDTO
            {
                BillToAddress1 = billToAddress1,
                BillToAdministrativeArea = billToAdministrativeArea,
                BillToCompany = billToCompany,
                BillToCountry = billToCountry,
                BillToEmail = billToEmail,
                BillToFirstName = billToFirstName,
                BillToLastName = billToLastName,
                BillToLocality = billToLocality,
                BillToPhoneNumber = billToPhoneNumber,
                BillToPostalCode = billToPostalCode,
                CardExpirationMonth = cardExpirationMonth,
                CardExpirationYear = cardExpirationYear,
                InstrumentIdentifierId = instrumentIdentifierId,
                PaymentInstrumentId = paymentInstrumentTokenId
            };

            using HttpClient httpClient = new HttpClient();
            StringContent content = new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json");

            try
            {
                using HttpResponseMessage response = await httpClient.PutAsync(url, content);
                string responseStream = await response.Content.ReadAsStringAsync();
                GenericResponse<Tmsv2customersEmbeddedDefaultPaymentInstrument> result = JsonConvert
                    .DeserializeObject<GenericResponse<Tmsv2customersEmbeddedDefaultPaymentInstrument>>(responseStream);

                if (result.IsSuccess)
                {
                    result.Result.Embedded = null;

                    string serializedObject = JsonConvert.SerializeObject(result.Result);
                    return RedirectToPage(
                       "SuccessResponseGeneric",
                       new
                       {
                           applicationName,
                           serializedObject,
                           isPayment = false,
                           accessCode,
                           customerId
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

        private PaymentInstrumentUpdateDTO ToPaymentInstrumentUpdateDTO(
            Tmsv2customersEmbeddedDefaultPaymentInstrument instr,
            string instrumentIdentifierId,
            string paymentInstrumentId)
        {
            return new PaymentInstrumentUpdateDTO
            {
                BillToAddress1 = instr.BillTo.Address1,
                BillToAdministrativeArea = instr.BillTo.AdministrativeArea,
                BillToCompany = instr.BillTo.Company,
                BillToCountry = instr.BillTo.Country,
                BillToEmail = instr.BillTo.Email,
                BillToFirstName = instr.BillTo.FirstName,
                BillToLastName = instr.BillTo.LastName,
                BillToLocality = instr.BillTo.Locality,
                BillToPhoneNumber = instr.BillTo.PhoneNumber,
                BillToPostalCode = instr.BillTo.PostalCode,
                CardExpirationMonth = instr.Card.ExpirationMonth,
                CardExpirationYear = instr.Card.ExpirationYear,
                InstrumentIdentifierId = instrumentIdentifierId,
                PaymentInstrumentId = paymentInstrumentId
            };
        }
    }
}
