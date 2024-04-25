using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using App.Data.Entities;

namespace App.Contracts;
public class SupplyOrderResponse
{
    public Guid Id { get; set; }
    public double TotalPrice { get; set; } = 0;
    public EmployeeContract Employee { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public SupplyOrderStatus Status { get; set; }
    public List<SupplyItemResponseOverview>? Items { get; set; }
}
