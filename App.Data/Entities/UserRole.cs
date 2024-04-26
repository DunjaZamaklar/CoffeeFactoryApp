using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace App.Data.Entities
{
    public class UserRole
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
       
    }
}
