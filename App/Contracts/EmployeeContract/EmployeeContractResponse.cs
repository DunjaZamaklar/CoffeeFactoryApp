using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using App.Data.Entities;

namespace App.Contracts;
public class EmployeeContractResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public Boolean ActiveFlag { get; set; }
    public Employee Employee { get; set; }
    public EmployeePosition EmployeePosition  { get; set; }
}
