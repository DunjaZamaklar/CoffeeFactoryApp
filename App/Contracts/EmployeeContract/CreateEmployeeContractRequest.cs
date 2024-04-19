using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using App.Data.Entities;
using System.Diagnostics.CodeAnalysis;

namespace App.Contracts;
public class CreateEmployeeContractRequest
{
    public string Type { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid EmployeePositionId { get; set; }
}