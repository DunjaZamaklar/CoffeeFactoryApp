using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using App.Data.Entities;

namespace App.Contracts;
public class CreateSupplyRequest
{
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; } = 0;
    public double Price { get; set; } = 0;
    public Guid SupplierId { get; set; }
    public Guid SupplyCategoryId { get; set; }
}