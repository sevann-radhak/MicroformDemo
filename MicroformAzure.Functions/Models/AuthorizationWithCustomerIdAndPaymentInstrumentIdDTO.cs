namespace MicroformAzure.Functions.Models
{
    public class AuthorizationWithCustomerIdAndPaymentInstrumentIdDTO
    {
        public string AccessCode { get; set; }
        public string CustomerId { get; set; }
        public string PaymentInstrumentId { get; set; }
        public string ShippingAddressId { get; set; }
    }
}
