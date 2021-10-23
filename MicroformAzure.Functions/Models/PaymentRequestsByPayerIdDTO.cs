using System;
using System.Collections.Generic;

namespace MicroformAzure.Functions.Models
{
    public class PaymentRequestsByPayerIdDTO
    {
        public string AccessCode { get; set; }
        public string PayerId { get; set; }
        //public string CustomerId { get; set; }
        public ICollection<RequestsByPayerIdDTO> Requests { get; set; }
        public string UrlRedirectAfterPayment { get; set; }
    }

    public class RequestsByPayerIdDTO
    {
        public string ApplicationName { get; set; }
        public string Currency { get; set; }
        public string OrderCode { get; set; }
        public DateTime? RequestCreatedTime { get; set; }
        public DateTime? ResultCreatedTime { get; set; }
        public string ReturnDesicion { get; set; }
        public string ReturnResult { get; set; }
        public string TotalAmount { get; set; }
    }
}
