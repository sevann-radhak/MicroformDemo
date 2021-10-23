using System;
using System.Collections.Generic;

namespace MicroformAzure.Web.Models
{
    public class ApplicationPayerInfoModel
    {
        public int Id { get; set; }

        public string AccessCode { get; set; }

        public bool IsAccessCodeAvailable { get; set; }

        public string ApplicationName { get; set; }

        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        public string CustomerId { get; set; }

        public string PayerId { get; set; }

        public virtual ICollection<ApplicationPayerTokenModel> ApplicationPayerTokens { get; set; }
    }
}
