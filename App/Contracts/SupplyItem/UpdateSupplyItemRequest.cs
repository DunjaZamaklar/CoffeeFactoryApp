using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using App.Data.Entities;

namespace App.Contracts;
public class UpdateSupplyItemRequest
{
    public double Quantity { get; set; } = 0;
    public Guid Supply { get; set; }
    public Guid SupplyOrderId { get; set; }
}