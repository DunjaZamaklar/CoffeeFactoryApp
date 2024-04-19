using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace App.Data.Entities
{
    public class EmployeeContract
    {
        [Key]
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public Boolean ActiveFlag { get; set; }
        public Employee Employee { get; set; }
        public EmployeePosition EmployeePosition { get; set; }

    }
}
