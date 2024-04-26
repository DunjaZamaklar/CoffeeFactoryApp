using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace App.Contracts;
public class UpdateRoleEmployeeRequest
{
    public Guid RoleId { get; set; } 
}