using AttendanceApp.Data;
using AttendanceApp.Model;
using AttendanceApp.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net.Mail;
using System.Net;
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

        //GET: EmployeeRegister List
        [HttpGet]
        [Route("GetEmployeeRegisterList")]
        public async Task<ActionResult<IEnumerable<EmployeeRegister>>> GetEmployeeRegisterAsync()
        {
            try
            {
                var employeeRegister = await _context.EmployeeRegister.ToListAsync();

                if (employeeRegister.Count == 0)
                {
                    var errorResponse = _errorResponseService.CreateErrorResponse(400, "No employee data found");
                    return BadRequest(errorResponse);
                }

                var response = new
                {
                    Status = 200,
                    Message = "Data found successfully",
                    Data = employeeRegister
                };

                return Ok(response);
            }
            catch (Exception)
            {
                var errorResponse = _errorResponseService.CreateErrorResponse(500, "Internal Server Error");
                return BadRequest(errorResponse);
            }
        }



        //GET: Profile By EmployeeId
        [HttpGet]
        [Route("GetProfileById/{id}")]
        public IActionResult GetProfileById(int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existEmployee = _context.EmployeeRegister.Find(id);

                    if (existEmployee == null)
                    {
                        var errorResponse = _errorResponseService.CreateErrorResponse(400, "Enter a valid EmployeeId");
                        return BadRequest(errorResponse);
                    }

                    var filePath = existEmployee.EmployeeProfile;

                    if (System.IO.File.Exists(filePath))
                    {
                        // Read the file into a byte array
                        byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

                        // Return the file as a byte array
                        return File(fileBytes, "image/png"); // Set the appropriate content type based on your file type
                    }
                }
                return BadRequest(ModelState);
            }
            catch (Exception)
            {
                var errorResponse = _errorResponseService.CreateErrorResponse(500, "Internal Server Error");
                return StatusCode(500, errorResponse);
            }
        }




        //GET: Employee By Email
        [HttpGet]
        [Route("GetEmployeeRegisterByEmail")]
        public async Task<ActionResult<IEnumerable<EmployeeRegister>>> GetEmployeeRegisterByEmail([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    var errorResponse = _errorResponseService.CreateErrorResponse(400, "Please provide a valid email");
                    return BadRequest(errorResponse);
                }

                var employeeRegisterList = await _context.EmployeeRegister
                    .Where(e => e.EmployeeEmail.ToLower() == email.ToLower())
                    .ToListAsync();

                if (employeeRegisterList.Count == 0)
                {
                    var errorResponse = _errorResponseService.CreateErrorResponse(404, "Please Enter a valid Email");
                    return NotFound(errorResponse);
                }

                var response = new
                {
                    Status = 200,
                    Message = "Data found successfully",
                    Data = employeeRegisterList
                };

                return Ok(response);
            }
            catch (Exception)
            {
                var errorResponse = _errorResponseService.CreateErrorResponse(500, "Internal Server Error");
                return BadRequest(errorResponse);
            }
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


        //<<<----Method to Upload File Starts--->>>
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

                return filePath;
            }
            catch (Exception)
            {
                return string.Empty;
                //Console.WriteLine($"File Not Stored in the Database. Error: {ex.Message}");
                //return string.Empty;
            }
            return filename;
        }

        //private void SaveFilePathToDatabase(string filePath)
        //{
        //    try
        //    {
        //        _context.SaveChanges();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("File not Stored into the Datbase");
        //    }

        //}
        //<<<----Method to Upload File Ends--->>>


        //<<<----Generate and Send OTP Starts--->>>
        [HttpPost("GenerateAndSendOTP")]
        public async Task<IActionResult> GenerateAndSendOTP(string employeeEmail)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if the EmployeeEmail already exists
                    var existingRequest = await _context.OTPRequested.FirstOrDefaultAsync(o => o.employeeEmail == employeeEmail);

                    if (existingRequest != null)
                    {

                        // Generate new OTP and update the existing request
                        existingRequest.OTP = new Random().Next(1000, 9999).ToString();
                        existingRequest.otpExpired = DateTime.Now.AddMinutes(5);
                        existingRequest.otpStatus = false;

                        //Explicitly properties as modified
                        _context.Entry(existingRequest).Property(x => x.OTP).IsModified = true;
                        _context.Entry(existingRequest).Property(x => x.otpExpired).IsModified = true;
                        _context.Entry(existingRequest).Property(x => x.otpStatus).IsModified = true;

                        existingRequest.lastUpdated = DateTime.Now;
                        _context.Entry(existingRequest).Property(x => x.lastUpdated).IsModified = true;

                        // Update the database
                        await _context.SaveChangesAsync();

                        // Send OTP to the employee's email
                        await SendEmail(employeeEmail, existingRequest.OTP);

                        return Ok(new { Message = "OTP regenerated and sent successfully. - Update" });

                    }
                    else
                    {
                        // If EmployeeEmail is not found, generate a new OTP
                        string otp = new Random().Next(1000, 9999).ToString();

                        // Save OTP to the database
                        var otpRequest = new OTPRequested
                        {
                            employeeEmail = employeeEmail,
                            OTP = otp,
                            otpExpired = DateTime.Now.AddMinutes(5), // Set OTP expiration time (e.g., 5 minutes)
                            otpStatus = false
                        };

                        _context.OTPRequested.Add(otpRequest);
                        await _context.SaveChangesAsync();

                        // Send OTP to the employee's email
                        await SendEmail(employeeEmail, otp);

                        return Ok(new { Message = "OTP generated and sent successfully. - New" });
                    }
                }
                return BadRequest();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        private async Task SendEmail(string toEmail, string otp)
        {
            // Replace these placeholders with your SMTP server details
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587;
            string smtpUsername = "test.nextolive2@gmail.com";
            string smtpPassword = "olgp xsdp quqk zynv";

            using (var client = new SmtpClient(smtpServer, smtpPort))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = true;

                try
                {
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("test.nextolive2@gmail.com", "NextOlive"),
                        Subject = "Your OTP for Verification",
                        Body = $"Your OTP is: {otp} will be Expired in 5 minutes."
                    };

                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                }
                catch (Exception)
                {
                    // Log or handle the exception appropriately
                    Console.WriteLine($"Error sending email");
                    throw;
                }
            }
        }
        //<<<----Generate and Send OTP Ends--->>>

        //PUT: Update EmployeeRegister
        //[HttpPut("{employeeEmail}")]
        //public async Task<IActionResult> UpdateEmployee(string employeeEmail, [FromBody] EmployeeRegister updatedEmployee)
        //{
        //    try
        //    {
        //        var existingEmployee = await _context.EmployeeRegister.FirstOrDefaultAsync(e => e.EmployeeEmail == employeeEmail);

        //        if (existingEmployee == null)
        //        {
        //            var errorResponse = _errorResponseService.CreateErrorResponse(404, "Employee not found");
        //            return NotFound(errorResponse);
        //        }

        //        // Update properties with non-null values - Values cannot be updated as null or white space
        //        // Update properties if they are present in the request
        //        if (updatedEmployee.DepartmentId != 0)
        //            existingEmployee.DepartmentId = updatedEmployee.DepartmentId;

        //        if (!string.IsNullOrWhiteSpace(updatedEmployee.EmployeeName))
        //            existingEmployee.EmployeeName = updatedEmployee.EmployeeName;

        //        if (!string.IsNullOrWhiteSpace(updatedEmployee.Phone))
        //            existingEmployee.Phone = updatedEmployee.Phone;

        //        if (!string.IsNullOrWhiteSpace(updatedEmployee.Address))
        //            existingEmployee.Address = updatedEmployee.Address;

        //        //if (updatedEmployee.EmployeeProfile != null)
        //        //    existingEmployee.EmployeeProfile = updatedEmployee.EmployeeProfile;

        //        // Validate and update other properties as needed

        //        _context.Entry(existingEmployee).State = EntityState.Modified;
        //        await _context.SaveChangesAsync();

        //        var response = new
        //        {
        //            Status = 200,
        //            Message = "Employee updated successfully",
        //            Data = existingEmployee
        //        };

        //        return Ok(response);
        //    }
        //    catch (Exception)
        //    {
        //        var errorResponse = _errorResponseService.CreateErrorResponse(500, "Internal Server Error");
        //        return StatusCode(500, errorResponse);
        //    }
        //}

    }
}
