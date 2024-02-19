using System.ComponentModel.DataAnnotations;

namespace AttendanceApp.Model
{
    public class EmployeeRegister
    {
        [Key]
        public int EmployeeId { get; set; }
        [Required(ErrorMessage ="DepartmentId is Mandatory")]
        public int DepartmentId { get; set; }
        public bool OTPStatus { get; set; } = false;
        [Required(ErrorMessage ="Name is Mandatory")]
        public string EmployeeName { get; set; }
        [Required(ErrorMessage ="Email is Mandatory")]
        public string EmployeeEmail { get; set; }
        [Required(ErrorMessage ="Password is Mandatory")]
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string? EmployeeProfile { get; set; }
        public DateTime LastUpdate { get; set; } = DateTime.Now;
        public int AccountType { get; set; } = 1;
    }
}
