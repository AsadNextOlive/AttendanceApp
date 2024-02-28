using System.ComponentModel.DataAnnotations;

namespace AttendanceApp.Model
{
    public class OTPRequested
    {
        [Key]
        public int otpRequestId { get; set; }
        [Required]
        public string employeeEmail { get; set; }
        public string OTP { get; set; }
        public bool otpStatus { get; set; }
        public DateTime otpExpired { get; set; }
        public DateTime lastUpdated { get; set; } = DateTime.Now;
    }
}
