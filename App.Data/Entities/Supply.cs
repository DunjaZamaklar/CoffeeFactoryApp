using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace App.Data.Entities
{
    public class Supply
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Quantity { get; set; } = 0;
        public double Price { get; set; } = 0;
        [NotNull]
        public Supplier Supplier { get; set; }
        [NotNull]
        public SupplyCategory SupplyCategory { get; set; }

    }
}
