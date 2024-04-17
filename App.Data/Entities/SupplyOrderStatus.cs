using System.ComponentModel.DataAnnotations;

namespace App.Data.Entities
{
    public class SupplyOrderStatus
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
