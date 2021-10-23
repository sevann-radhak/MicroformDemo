using System;

namespace MicroformAzure.Web.Models
{
    public class ScheduledPaymentsDTO
    {
        public string Param1 { get; set; }
        public string Param2 { get; set; }
        public string Param3 { get; set; }
        public string Frequency { get; set; }
        public DateTime ExecutionExact { get; set; }
    }
}