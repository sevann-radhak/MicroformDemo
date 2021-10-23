using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MicroformAzure.Functions.Entities
{
    public class ApplicationLogsEntity : IEntity
    {
        [Key]
        public int Id { get; set; }

        public int RelatedEventId { get; set; }

        public string Message { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    }
}
