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
    public class PaymentInstrumentCreateModel : PageModel
    {
        public string ApplicationName { get; set; }
        public string CustomerId { get; set; }
        public PaymentInstrumentCreateDTO PaymInstr { get; set; }

        private readonly ILogger<PaymentInstrumentCreateModel> _logger;
        private IConfiguration Configuration { get; }
        private string UrlFunctions { get; set; }

        public PaymentInstrumentCreateModel(
            IConfiguration configuration,
            ILogger<PaymentInstrumentCreateModel> logger)
        {
            _logger = logger;
            Configuration = configuration;
            PaymInstr = new PaymentInstrumentCreateDTO();
        }

        public void OnGet(
            string accessCode,
            string applicationName,
            string customerId)
        {
            UrlFunctions = Configuration["UrlFunctions"];
            PaymInstr.AccessCode = accessCode;
            ApplicationName = applicationName;
            CustomerId = customerId;
        }

        public async Task<IActionResult> OnPost()
        {
            UrlFunctions = Configuration["UrlFunctions"];

            StringValues accessCode = Request.Form["accessCode"];
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
            StringValues cardNumber = Request.Form["cardNumber"];
            StringValues customerId = Request.Form["customerId"];

            string url = $"{UrlFunctions}/api/customers/{customerId}/payment-instruments";

            PaymentInstrumentCreateDTO request = new PaymentInstrumentCreateDTO
            {
                AccessCode = accessCode,
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
                CardNumber = cardNumber,
                CustomerId = customerId
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
                    return RedirectToPage(
                       "SuccessResponseGeneric",
                       new
                       {
                           applicationName,
                           serializedObject,
                           isPayment = true,
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
