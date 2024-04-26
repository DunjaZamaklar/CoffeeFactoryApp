using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace App.Contracts;
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    [PasswordPropertyText]
    public string Password { get; set; } = string.Empty;

}