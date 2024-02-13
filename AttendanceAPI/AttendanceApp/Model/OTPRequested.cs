using System.ComponentModel.DataAnnotations;

namespace AttendanceApp.Model
{
    public class OTPRequested
    {
        [Key]
        public int otpRequestId { get; set; }
        [Required]
        public string employeeEmail { get; set; }
        public int OTP { get; set; }
        public string otpStatus { get; set; }
        public DateTime otpExpired { get; set; }
        public DateTime lastUpdated { get; set; } = DateTime.Now;
    }
}
