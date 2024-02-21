using AttendanceApp.Data;
using AttendanceApp.Model;
using AttendanceApp.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ErrorResponseService _errorResponseService;

        public LoginController(ApplicationDbContext context, ErrorResponseService errorResponseService)
        {
            _context = context;
            _errorResponseService = errorResponseService;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    login.isLoggedIn = false;
                    var loggedIn = await _context.EmployeeRegister.FirstOrDefaultAsync(x => x.EmployeeEmail == login.Email && x.Password == login.Password);

                    //Check if Username or password is invalid
                    if (loggedIn == null)
                    {
                        login.isLoggedIn = false;
                        var errorResponse = _errorResponseService.CreateErrorResponse(400, "Username or password is invalid");
                        return BadRequest(errorResponse);
                    }

                    //Check if User is not verified | OTPStatus = False
                    if (!loggedIn.OTPStatus)
                    {
                        login.isLoggedIn = false;
                        var errorResponse = _errorResponseService.CreateErrorResponse(400, "User is not verified");
                        return BadRequest(errorResponse);
                    }

                    login.isLoggedIn = true;
                    var response = new
                    {
                        Status = 200,
                        Message = "Login Successful",
                        Data = new
                        {
                            login.Email,
                            login.isLoggedIn,
                            loggedIn.AccountType //Retrieving the AccountType for Admin or Employee
                        }
                    };
                    return Created("", response);
                }
                catch (Exception ex)
                {
                    var errorResponse = _errorResponseService.CreateErrorResponse(500, $"Internal Server Error - ERROR: {ex}");
                    return BadRequest(errorResponse);
                }
            }
            return BadRequest(ModelState);
        }

        //POST: SignOut
        [HttpPost]
        [Route("SignOut")]
        public IActionResult SignOut()
        {
            try
            {
                var login = new Login
                {
                    isLoggedIn = false
                };

                var response = new
                {
                    Status = 200,
                    Message = "Sign Out Successful",
                    Data = new
                    {
                        login.isLoggedIn
                    }
                };

                return Created("", response);
            }
            catch (Exception)
            {
                var errorResponse = _errorResponseService.CreateErrorResponse(500, "Internal Server Error");
                return BadRequest(errorResponse);
            }
        }
    }
}
