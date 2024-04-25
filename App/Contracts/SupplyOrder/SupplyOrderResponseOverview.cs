using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using App.Data.Entities;

namespace App.Contracts;
public class SupplyOrderResponseOverview
{
    public Guid Id { get; set; }
    public double TotalPrice { get; set; } = 0;
    public EmployeeContract Employee { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public SupplyOrderStatus Status { get; set; }
}
