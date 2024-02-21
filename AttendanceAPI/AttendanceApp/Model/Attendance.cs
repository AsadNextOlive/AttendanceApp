using System.ComponentModel.DataAnnotations;

namespace AttendanceApp.Model
{
    public class Attendance
    {
        [Key]
        public int AttendanceId { get; set; }
        [Required(ErrorMessage = "DepartmentId is Mandatory")]
        public int DepartmentId { get; set; } //Foreign Key

        [Required(ErrorMessage = "AttendanceDate is Mandatory")]
        [DataType(DataType.Date)]
        public DateTime AttendanceDate { get; set; }

        [Required(ErrorMessage = "EmployeeId is Mandatory")]
        public int EmployeeId { get; set; } //Foreign Key

        [Required(ErrorMessage = "EmployeeEmail is Mandatory")]
        public string EmployeeEmail { get; set; } //Foreign Key

        [Required(ErrorMessage = "EmployeeName is Mandatory")]
        public string EmployeeName { get; set; } //Foreign Key

        [Required(ErrorMessage = "AttendanceStatus is Mandatory")]
        public AttendanceStatus AttendanceStatus { get; set; } = AttendanceStatus.Absent;

        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }

    //Non-Primitive Data Type Declaration
    public enum AttendanceStatus
    {
        Present,
        Absent,
        Leave
    }
}
