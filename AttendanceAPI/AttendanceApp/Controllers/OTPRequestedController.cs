using AttendanceApp.Data;
using AttendanceApp.Model;
using AttendanceApp.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OTPRequestedController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ErrorResponseService _errorResponseService;

        public OTPRequestedController(ApplicationDbContext context, ErrorResponseService errorResponseService)
        {
            _context = context;
            _errorResponseService = errorResponseService;
        }

        [HttpPost("VerifyOTP")]
        public IActionResult VerifyOTP(string employeeEmail, string OTP)
        {
            try
            {
                // Check if the EmployeeEmail exists
                var existingRequest = _context.OTPRequested.FirstOrDefault(o => o.employeeEmail == employeeEmail);

                if (existingRequest != null)
                {
                    // Check if OTP matches and it's not expired
                    if (existingRequest.OTP == OTP && DateTime.Now <= existingRequest.otpExpired)
                    {
                        //Updating otpStatus in EmployeeRegister Table post varification
                        existingRequest.otpStatus = true;
                        _context.Entry(existingRequest).Property(x => x.otpStatus).IsModified = true;

                        var registerEmployee = _context.EmployeeRegister.FirstOrDefault(e => e.EmployeeEmail == employeeEmail);
                        if (registerEmployee != null)
                        {
                            registerEmployee.OTPStatus = true;
                            _context.Entry(registerEmployee).Property(x => x.OTPStatus).IsModified = true;
                        }

                        _context.SaveChanges();

                        var response = new
                        {
                            Satus = 200,
                            Message = "OTP verification successful",
                            Data = new
                            {
                                existingRequest.otpStatus
                            }
                        };
                        return Ok(response);
                        //return Ok(new { Message = "OTP verification successful." });
                    }
                    if (DateTime.Now > existingRequest.otpExpired)
                    {
                        var errorResponse = _errorResponseService.CreateErrorResponse(400, "OTP Expired");
                        return BadRequest(errorResponse);
                    }
                }
                else
                {
                    var errorResponse = _errorResponseService.CreateErrorResponse(404, "Could not found Employee Data");
                    return BadRequest(errorResponse);
                    //return BadRequest(new { Message = "Employee email not found." });
                }
                return BadRequest(ModelState);
            }
            catch (Exception)
            {
                var errorResponse = _errorResponseService.CreateErrorResponse(500, "Internal Server Error");
                return BadRequest(errorResponse);
            }
        }
    }
}
