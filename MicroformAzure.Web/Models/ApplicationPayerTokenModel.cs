using System;

namespace MicroformAzure.Web.Models
{
    public class ApplicationPayerTokenModel
    {
        //public int Id { get; set; }

        //public string CardInfo { get; set; }

        //public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        //public string PaymentMethod { get; set; }

        //public string Token { get; set; }

        //public DateTime UpdatedTime { get; set; }

        //public ApplicationPayerInfoModel ApplicationPayerInfo { get; set; }


        public string CustomerId { get; set; }
        public string PaymentInstrumentId { get; set; }
        public string ShippingAddressId { get; set; }
        //public string Id { get; set; }
        public InstrumentIdentifierModel InstrumentIdentifier { get; set; }
        public PaymentInstrumentUpdateDTO PaymentInstrumentInfo { get; set; }
        public string State { get; set; }
    }
}
