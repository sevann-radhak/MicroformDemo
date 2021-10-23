using CyberSource.Model;
using System;
using System.Collections.Generic;

namespace MicroformAzure.Functions.Models
{
    public class RespopnseWithAccessCodeDTO
    {
        public string AccessCode { get; set; }
        public string ApplicationName { get; set; }
        public string ClientReferenceInformationCode { get; set; }
        //public CustomerModel_OLD CustomerModel { get; set; }
        public DateTime ExecutionExact { get; set; }
        public FlexV1KeysPost200Response FlexResponse { get; set; }
        public ICollection<PaymentInstrumentModel> PaymentInstruments { get; set; }
    }
}
