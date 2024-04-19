using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace App.Contracts;
public class SupplierResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
