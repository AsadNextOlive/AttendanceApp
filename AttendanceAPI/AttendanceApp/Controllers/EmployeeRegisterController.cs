using AttendanceApp.Data;
using AttendanceApp.Model;
using AttendanceApp.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AttendanceApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeRegisterController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ErrorResponseService _errorResponseService;
        private readonly ValidationService _validationService;

        public EmployeeRegisterController(ApplicationDbContext context, ErrorResponseService errorResponseService, ValidationService validationService)
        {
            _context = context;
            _errorResponseService = errorResponseService;
            _validationService = validationService;
        }

        //POST: api/EployeeRegister
        [HttpPost]
        public async Task<ActionResult<EmployeeRegister>> EmployeeRegister(EmployeeRegister employeeRegister)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //Check if Department is selected
                    var department = await _context.Department.FirstOrDefaultAsync(d => d.DepartmentId == employeeRegister.DepartmentId);
                    if (department == null)
                    {
                        var errorResponse = _errorResponseService.CreateErrorResponse(400, "Please enter valid Department Id");
                        return BadRequest(errorResponse);
                    }

                    //Check if Email is valid
                    if (!_validationService.IsValidEmail(employeeRegister.EmployeeEmail))
                    {
                        var errorResponse = _errorResponseService.CreateErrorResponse(400, "Please Enter valid Email");
                        return BadRequest(errorResponse);
                    }

                    //Check if user is already registered
                    var existEmployee = await _context.EmployeeRegister.FirstOrDefaultAsync(e => e.EmployeeEmail == employeeRegister.EmployeeEmail);
                    if (existEmployee != null)
                    {
                        var errorResponse = _errorResponseService.CreateErrorResponse(400, "Email Already Exist");
                        return BadRequest(errorResponse);
                    }

                    //Check password length
                    if (string.IsNullOrWhiteSpace(employeeRegister.Password) || employeeRegister.Password.Length < 6)
                    {
                        var errorResponse = _errorResponseService.CreateErrorResponse(400, "Password must be at least 6 character");
                        return BadRequest(errorResponse);
                    }

                    //Check if password is not strong
                    if (!_validationService.IsAlphanumeric(employeeRegister.Password))
                    {
                        var errorResponse = _errorResponseService.CreateErrorResponse(400, "Password must be Alphanumeric including special character");
                        return BadRequest(errorResponse);
                    }

                    _context.EmployeeRegister.Add(employeeRegister);
                    await _context.SaveChangesAsync();

                    var response = new
                    {
                        Status = 200,
                        Message = "Registered Successfully",
                        Data = employeeRegister
                    };

                    return Created("", response);
                }
                return BadRequest(ModelState);
            }
            catch (Exception)
            {
                var errorResponse = _errorResponseService.CreateErrorResponse(500, "Internal Server Error");
                return StatusCode(500, errorResponse);
            }
        }
        
    }
}
