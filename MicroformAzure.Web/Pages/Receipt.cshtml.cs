using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MicroformAzure.Web.Pages
{
    public class ReceiptModel : PageModel
    {
        public string ApplicationName { get; set; }
        public string Id { get; set; }
        public string Customerid { get; set; }
        public string Instrumentidentifierid { get; set; }
        public string Paymentinstrumentid { get; set; }
        public string Status { get; set; }

        public void OnGet(
            string id,
            string customerid,
            string instrumentidentifierid,
            string paymentinstrumentid,
            string status,
            string applicationName)
        {
            ApplicationName = applicationName;
            Id = id;
            Customerid = customerid;
            Instrumentidentifierid = instrumentidentifierid;
            Paymentinstrumentid = paymentinstrumentid;
            Status = status;
        }
    }
}
