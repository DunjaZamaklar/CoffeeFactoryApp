using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using App.Data.Entities;

namespace App.Contracts;
public class SupplyItemResponseOverview
{
    public Guid Id { get; set; }
    public double Quantity { get; set; } = 0;
    public double Price { get; set; } = 0;
    public Supply Supply { get; set; }
}
