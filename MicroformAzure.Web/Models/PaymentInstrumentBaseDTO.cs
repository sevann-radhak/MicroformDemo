namespace MicroformAzure.Web.Models
{
    public class PaymentInstrumentBaseDTO
    {
        public string BillToFirstName { get; set; }
        public string BillToLastName { get; set; }
        public string BillToCompany { get; set; }
        public string BillToAddress1 { get; set; }
        public string BillToLocality { get; set; }
        public string BillToAdministrativeArea { get; set; }
        public string BillToPostalCode { get; set; }
        public string BillToCountry { get; set; }
        public string BillToEmail { get; set; }
        public string BillToPhoneNumber { get; set; }
        public string CardExpirationMonth { get; set; }
        public string CardExpirationYear { get; set; }
    }
}
