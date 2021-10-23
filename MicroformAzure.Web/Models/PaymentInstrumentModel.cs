namespace MicroformAzure.Web.Models
{
    public class PaymentInstrumentModel
    {
        public string Id { get; set; }
        public InstrumentIdentifierModel InstrumentIdentifier { get; set; }
        public PaymentInstrumentUpdateDTO PaymentInstrumentInfo { get; set; }
        public string State { get; set; }
    }
}
