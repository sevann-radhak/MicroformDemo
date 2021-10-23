using System;
using System.ComponentModel.DataAnnotations;

namespace MicroformAzure.Functions.Entities
{
    public class PaymentResultEntity : IEntity
    {
        [Key]
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        public string ReturnDesicion { get; set; }

        public string ReturnResult { get; set; }

        public PaymentRequestEntity PaymentRequest { get; set; }

        public string Status { get; set; }
    }
}
