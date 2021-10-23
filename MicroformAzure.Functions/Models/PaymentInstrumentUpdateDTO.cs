using System.ComponentModel.DataAnnotations;

namespace MicroformAzure.Functions.Models
{
    public class PaymentInstrumentUpdateDTO : PaymentInstrumentBaseDTO
    {
        //[Required(ErrorMessage = "The field {0} is mandatory")]
        public string CustomerId { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory")]
        public string InstrumentIdentifierId { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory")]
        public string PaymentInstrumentId { get; set; }

        //[Required(ErrorMessage = "The field {0} is mandatory")]
        public string ShippingAddressId { get; set; }
    }
}
