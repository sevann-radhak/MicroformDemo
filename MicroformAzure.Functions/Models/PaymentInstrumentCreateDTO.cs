using System.ComponentModel.DataAnnotations;

namespace MicroformAzure.Functions.Models
{
    public class PaymentInstrumentCreateDTO : PaymentInstrumentBaseDTO
    {
        [Required(ErrorMessage = "The field {0} is mandatory")]
        public string AccessCode { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory")]
        public string CustomerId { get; set; }
    }
}
