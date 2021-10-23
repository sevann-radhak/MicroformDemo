namespace MicroformAzure.Web.Models
{
    public class PaymenteCheckDTO
    {
        public string AccessCode { get; set; }
        public string BillToFirstName { get; set; }
        public string BillToLastName { get; set; }
        public string BillToAddress1 { get; set; }
        public string BillToLocality { get; set; }
        public string BillToAdministrativeArea { get; set; }
        public string BillToPostalCode { get; set; }
        public string BillToCountry { get; set; }
        public string BillToEmail { get; set; }
        public string BankAccountType { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankRoutingNumber { get; set; }
    }
}
