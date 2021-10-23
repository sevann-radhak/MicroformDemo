using MicroformAzure.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace MicroformAzure.Web.Pages
{
    public class SuccessResponseGenericModel : PageModel
    {
        public string ApplicationName { get; set; }
        public object GenericObject { get; set; }
        public string UrlRedirectAfterPayment { get; set; }

        public void OnGet(
            string applicationName, 
            string serializedObject,
            bool isPayment = false,
            string accessCode = null,
            string customerId = null)
        {
            ApplicationName = applicationName;
            GenericObject = JsonConvert.DeserializeObject(serializedObject);
            
            if (isPayment)
            {
                PaymentResponseDTO payment = JsonConvert.DeserializeObject<PaymentResponseDTO>(serializedObject);
                UrlRedirectAfterPayment = payment.UrlRedirectAfterPayment;
            }
            else
            {
                UrlRedirectAfterPayment = $"https://paymentgatewaywebsc.azurewebsites.net/Result?" +
                    $"&applicationName={applicationName}" +
                    $"&status=OK";
            }
        }
    }
}
