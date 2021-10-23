using MicroformAzure.Functions.Entities;
using System.Collections.Generic;

namespace MicroformAzure.Functions.Models
{
    public class CustomerModel_OLD
    {
        public string Id { get; set; }
        public string ClientReferenceInformationCode { get; set; }
        public string Creator { get; set; }
        public ICollection<PaymentInstrumentModel> PaymentInstruments { get; set; }
        public string ShippingAddressId { get; set; }
    }

    //public class ApplicationPayerTokenModel //: ApplicationPayerTokenCybersource
    //{
    //    public string CustomerId { get; set; }
    //    public string PaymentInstrumentId { get; set; }
    //    public string ShippingAddressId { get; set; }
    //    //public string Id { get; set; }
    //    public InstrumentIdentifierModel InstrumentIdentifier { get; set; }
    //    public PaymentInstrumentUpdateDTO PaymentInstrumentInfo { get; set; }
    //    public string State { get; set; }
    //}
}
