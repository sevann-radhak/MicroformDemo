namespace MicroformAzure.Functions.Models
{
    public class PaymentInstrumentModel
    {
        public string Id { get; set; }
        public InstrumentIdentifierModel InstrumentIdentifier { get; set; }
        public PaymentInstrumentUpdateDTO PaymentInstrumentInfo { get; set; }
        public string State { get; set; }




        //public string CustomerId { get; set; }
        //public string PaymentInstrumentId { get; set; }
        //public string ShippingAddressId { get; set; }
    }
}
