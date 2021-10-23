using System.ComponentModel.DataAnnotations;

namespace MicroformAzure.Functions.Entities
{
    public interface IEntity
    {
        [Key]
        public int Id { get; set; }
    }
}
