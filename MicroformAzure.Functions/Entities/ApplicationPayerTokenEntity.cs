using System;
using System.ComponentModel.DataAnnotations;

namespace MicroformAzure.Functions.Entities
{
    public class ApplicationPayerTokenEntity : ApplicationPayerTokenCybersource, IEntity
    {
        [Key]
        public int Id { get; set; }

        public string CardInfo { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        public string Token { get; set; }

        [DataType(DataType.Date)]
        public DateTime UpdatedTime { get; set; }

        public ApplicationPayerInfoEntity ApplicationPayerInfo { get; set; }
    }

    public class ApplicationPayerTokenCybersource
    {
        public string CustomerId { get; set; }

        public string InstrumentIdentifierId { get; set; }

        public string PaymentInstrumentId { get; set; }

        public string ShippingAddressId { get; set; }
    }
}
