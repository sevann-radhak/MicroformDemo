using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroformAzure.Functions.Models
{
    public class AccessPaymentRequestDTO
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string ApplicationAccessKey { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string ApplicationName { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Currency { get; set; }
        
        public DateTime? ExecutionExact { get; set; }

        public string Language { get; set; }

        public string OrderCode { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string PaymentMethodsWithFee { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string PaymentOffice { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string PayerId { get; set; }

        public string UrlRedirectAfterPayment { get; set; }
    }
}
