using System.ComponentModel.DataAnnotations;

namespace AttendanceApp.Model
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }
        [Required(ErrorMessage ="Department Name is Mandatory")]
        public string DepartmentName { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}
