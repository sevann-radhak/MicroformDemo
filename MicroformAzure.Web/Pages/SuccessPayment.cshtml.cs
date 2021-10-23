using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MicroformAzure.Web.Pages
{
    public class SuccessPaymentModel : PageModel
    {
        public string Amount { get; set; }
        public string ApplicationName { get; set; }
        public string Status { get; set; }
        public string UserId { get; set; }

        public void OnGet(
            string applicationName,
            string status,
            string userId,
            string amount)
        {
            ApplicationName = applicationName;
            Amount = amount;
            Status = status;
            UserId = userId;
        }
    }
}
