namespace MicroformAzure.Web.Models
{
    public class PaymentInstrumentCreateDTO : PaymentInstrumentBaseDTO
    {
        public string AccessCode { get; set; }
        public string CardNumber { get; set; }
        public string CustomerId { get; set; }
    }
}
