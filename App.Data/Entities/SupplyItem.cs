using App.Data.Migrations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace App.Data.Entities
{
    public class SupplyItem
    {
        [Key]
        public Guid Id { get; set; }
        public double Quantity { get; set; } = 0;
        public double Price { get; set; } = 0;
        public Supply Supply { get; set; }
        public SupplyOrder SupplyOrder { get; set; }

    }
}
