using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace App.Contracts;
public class UpdatePasswordEmployeeRequest
{
    public string? Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}