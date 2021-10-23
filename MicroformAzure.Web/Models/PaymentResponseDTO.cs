namespace MicroformAzure.Web.Models
{
    public class PaymentResponseDTO
    {
        public string AccessCode { get; set; }
        public string AuthorizedAmount { get; set; }
        public string ClientReferenceCode { get; set; }
        public string Currency { get; set; }
        public string CustomerId { get; set; }
        public string Id { get; set; }
        public string InstrumentIdentifierId { get; set; }
        public string PaymentInstrumentId { get; set; }
        public string Status { get; set; }
        public string UrlRedirectAfterPayment { get; set; }
        public string TotalAmount { get; set; }
    }
}
