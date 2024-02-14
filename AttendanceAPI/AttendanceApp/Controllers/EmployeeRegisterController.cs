using AttendanceApp.Data;
using AttendanceApp.Model;
using AttendanceApp.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AttendanceApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
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
        public async Task<ActionResult<EmployeeRegister>> EmployeeRegister([FromForm] EmployeeRegister employeeRegister)  //Modifying Method for File Upload
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

                    // Check if a file is uploaded
                    var file = HttpContext.Request.Form.Files.FirstOrDefault();
                    if (file == null)
                    {
                        var errorResponse = _errorResponseService.CreateErrorResponse(400, "Please Upload Profile");
                        return BadRequest(errorResponse);
                    }
                    if (file != null)
                    {
                        // Save the file and get the filename
                        string filename = await WriteFile(file);

                        // Set the file path in the EmployeeRegister model
                        employeeRegister.EmployeeProfile = filename;
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
                var errorResponse = _errorResponseService.CreateErrorResponse(500, $"Internal Server Error");
                return StatusCode(500, errorResponse);
            }
        }

        //Post Method to Upload File
        private async Task<string> WriteFile(IFormFile file)
        {
            string filename = "";
            try
            {
                var extension = "." + file.FileName.Split('.').LastOrDefault();
                filename = $"{DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture)}{extension}";

                var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads\\Files");

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var filePath = Path.Combine(directoryPath, filename);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Save the full file path to the database
                //SaveFilePathToDatabase(filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File Not Stored in the Database. Error: {ex.Message}");
                return string.Empty;
            }
            return filename;
        }

        private void SaveFilePathToDatabase(string filePath)
        {
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("File not Stored into the Datbase");
            }

        }
    }
}
