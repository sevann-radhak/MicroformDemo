using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroformAzure.Functions.Entities
{
    public class ScheduledPaymentsEntity : IEntity
    {
        [Key]
        public int Id { get; set; }        

        public string Param1 { get; set; }

        public string Param2 { get; set; }

        public string Param3 { get; set; }

        public string Frequency { get; set; } // D=daily W=Weekly M = Montly

        [DataType(DataType.Date)]
        public DateTime LastExecution { get; set; } = DateTime.UtcNow;

        [DataType(DataType.Date)]
        public DateTime ExecutionExact { get; set; } = DateTime.UtcNow;

        public string Status { get; set; }

         [DataType(DataType.Date)]
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public decimal amount { get; set; }
        [NotMapped]
        public string applicationAccessKey { get; set; }
        [NotMapped]
        public string applicationName { get; set; }
        [NotMapped]
        public string applicationPayerId { get; set; }
        [NotMapped]
        public string currency { get; set; }
        [NotMapped]
        public string language { get; set; }
        [NotMapped]
        public string paymentMethodsWithFee { get; set; }
        [NotMapped]
        public string paymentOffice { get; set; }
        [NotMapped]
        public string payerId { get; set; }
                     


    }
}
