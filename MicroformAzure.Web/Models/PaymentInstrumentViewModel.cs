namespace MicroformAzure.Web.Models
{
    public class PaymentInstrumentViewModel
    {
        public string Id { get; set; }
        //public CardViewModel Card { get; set; }
        public InstrumentIdentifierViewModel InstrumentIdentifier { get; set; }
        public PaymentInstrumentUpdateDTO PaymentInstrumentInfo { get; set; }
        public string ShippingAddressId { get; set; }
        public string State { get; set; }
    }
}
