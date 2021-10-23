using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MicroformAzure.Functions.Entities
{
    public class PaymentRequestEntity : IEntity
    {
        [Key]
        public int Id { get; set; }

        public string BillingInfo { get; set; }

        public string CardInfo { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        public string PaymentMethod { get; set; }

        public ApplicationRequestEntity ApplicationRequest { get; set; }

        public ICollection<PaymentResultEntity> PaymentResults { get; set; }
    }
}
