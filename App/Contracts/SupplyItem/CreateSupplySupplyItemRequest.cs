using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using App.Data.Entities;

namespace App.Contracts;
public class CreateSupplySupplyItemRequest
{
    public double Quantity { get; set; } = 0;
    public Guid SupplyId { get; set; }
    public Guid SupplyOrderId { get; set; }
}