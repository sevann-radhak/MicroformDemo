using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MicroformAzure.Functions.Entities
{
    public class ApplicationPayerInfoEntity : IEntity
    {
        [Key]
        public int Id { get; set; }

        public string AccessCode { get; set; }

        public bool IsAccessCodeAvailable { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        public string PayerId { get; set; }

        public virtual ICollection<ApplicationPayerTokenEntity> ApplicationPayerTokens { get; set; }

        public virtual ICollection<ApplicationRequestEntity> ApplicationRequests { get; set; }
    }
}
