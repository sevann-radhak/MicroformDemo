using System;
using System.ComponentModel.DataAnnotations;

namespace MicroformAzure.Functions.Entities
{
    public class ApplicationSetupEntity : IEntity
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string ApplicationKey { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string ApplicationName { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; }

        [DataType(DataType.Date)]
        public DateTime? UpdatedTime { get; set; }
    }
}
