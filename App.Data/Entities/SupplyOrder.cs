using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace App.Data.Entities
{
    public class SupplyOrder
    {
        [Key]
        public Guid Id { get; set; }
        public double TotalPrice { get; set; } = 0;
        public EmployeeContract Employee { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public SupplyOrderStatus Status { get; set; }

    }
}
