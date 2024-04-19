using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using App.Data.Entities;

namespace App.Contracts;
public class SupplyResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; } = 0;
    public double Price { get; set; } = 0;
    public Supplier Supplier { get; set; }
    public SupplyCategory SupplyCategory { get; set; }
}
