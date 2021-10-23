namespace MicroformAzure.Web.Models
{
    public class PaymentInstrumentUpdateDTO : PaymentInstrumentBaseDTO
    {
        public string CustomerId { get; set; }
        public string InstrumentIdentifierId { get; set; }
        public string PaymentInstrumentId { get; set; }
        public string ShippingAddressId { get; set; }
    }
}
