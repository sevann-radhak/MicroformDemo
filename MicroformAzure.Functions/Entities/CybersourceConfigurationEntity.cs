using System.ComponentModel.DataAnnotations;

namespace MicroformAzure.Functions.Entities
{
    public class CybersourceConfigurationEntity : IEntity
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string AuthenticationType { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string EnableLog { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string KeyAlias { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string KeysDirectory { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string KeyFilename { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string KeyPass { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string MerchantID { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string MerchantKeyId { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string MerchantsecretKey { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string RunEnvironment { get; set; }
    }
}
