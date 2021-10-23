using System;
using System.Collections.Generic;

namespace MicroformAzure.Web.Models
{
    public class CustomerModel
    {
        public string Id { get; set; }
        public string ClientReferenceInformationCode { get; set; }
        public string Creator { get; set; }
        public ICollection<PaymentInstrumentModel> PaymentInstruments { get; set; }
        public string ShippingAddressId { get; set; }
        public DateTime ExecutionExact { get; set; }

    }
}
