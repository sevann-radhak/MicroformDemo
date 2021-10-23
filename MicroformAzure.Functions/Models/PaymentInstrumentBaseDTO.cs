using System.ComponentModel.DataAnnotations;

namespace MicroformAzure.Functions.Models
{
    public class PaymentInstrumentBaseDTO
    {
        [Required(ErrorMessage = "The field {0} is mandatory")]
        public string BillToFirstName { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory")]
        public string BillToLastName { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory")]
        public string BillToCompany { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory")]
        public string BillToAddress1 { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory")]
        public string BillToLocality { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory")]
        public string BillToAdministrativeArea { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory")]
        public string BillToPostalCode { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory")]
        public string BillToCountry { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory")]
        public string BillToEmail { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory")]
        public string BillToPhoneNumber { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory")]
        public string CardExpirationMonth { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory")]
        public string CardExpirationYear { get; set; }
    }
}
