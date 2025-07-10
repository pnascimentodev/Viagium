using System.ComponentModel.DataAnnotations;

namespace Viagium.Models
{
    public class Employees
    {
        [Key]
        public int EmployeeId { get; set; }
        public int Registration { get; set; }
        public string? RoleName { get; set; }
    }
}
