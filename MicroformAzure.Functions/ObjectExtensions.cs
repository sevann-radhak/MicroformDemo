using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MicroformAzure.Functions
{
    public static class ObjectExtensions
    {
        public static bool IsValid(this object o, out ICollection<ValidationResult> validationResults)
        {
            validationResults = new List<ValidationResult>();
            return Validator.TryValidateObject(o, new ValidationContext(o, null, null), validationResults, true);
        }
    }
}
