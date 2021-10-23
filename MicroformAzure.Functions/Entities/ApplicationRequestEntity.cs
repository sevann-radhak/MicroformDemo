using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroformAzure.Functions.Entities
{
    public class ApplicationRequestEntity : IEntity
    {
        [Key]
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public string ApplicationName { get; set; }

        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        public string Currency { get; set; }

        public string Language { get; set; }

        public string Message { get; set; }

        public string OfficeName { get; set; }

        public string OrderCode { get; set; }

        public string PaymentMethod { get; set; }

        public string ReferenceId { get; set; }

        public string TransactionCode { get; set; }

        public string UrlRedirectAfterPayment { get; set; }

        public ApplicationPayerInfoEntity ApplicationPayerInfo { get; set; }

        public ICollection<PaymentRequestEntity> PaymentRequests { get; set; }
        
        public DateTime? ExecutionExact { get; set; }

    }
}
