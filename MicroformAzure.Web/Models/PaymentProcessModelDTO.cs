using CyberSource.Model;
using System;
using System.Collections.Generic;

namespace MicroformAzure.Web.Models
{
    public class PaymentProcessModelDTO
    {
        public string AccessCode { get; set; }
        public string ApplicationName { get; set; }
        public string ClientReferenceInformationCode { get; set; }
        public DateTime ExecutionExact { get; set; }
        //public CustomerModel CustomerModel { get; set; }
        public FlexV1KeysPost200Response FlexResponse { get; set; }
        public ICollection<PaymentInstrumentModel> PaymentInstruments { get; set; }
    }
}
