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
    public class AttendanceController : ControllerBase
    {
        protected readonly ApplicationDbContext _context;
        protected readonly ErrorResponseService _errorResponseService;

        public AttendanceController(ApplicationDbContext context, ErrorResponseService errorResponseService)
        {
            _context = context;
            _errorResponseService = errorResponseService;
        }

        [HttpPost]
        public async Task<ActionResult<Attendance>> MarkAttendance([FromForm] Attendance attendance)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if Department is selected
                    var department = await _context.Department.FirstOrDefaultAsync(d => d.DepartmentId == attendance.DepartmentId);
                    if (department == null)
                    {
                        var errorResponse = _errorResponseService.CreateErrorResponse(400, "Please select valid Department Id");
                        return BadRequest(errorResponse);
                    }

                    // Check if employee is registered
                    var employee = await _context.EmployeeRegister.FirstOrDefaultAsync(e => e.EmployeeId == attendance.EmployeeId);
                    if (employee == null)
                    {
                        var errorResponse = _errorResponseService.CreateErrorResponse(400, "Please select valid Employee Id");
                        return BadRequest(errorResponse);
                    }

                    // Check the correct EmployeeEmail
                    var employeeEmail = await _context.EmployeeRegister.FirstOrDefaultAsync(e => e.EmployeeEmail == attendance.EmployeeEmail);
                    if (employeeEmail == null)
                    {
                        var errorResponse = _errorResponseService.CreateErrorResponse(400, "Please select valid Employee Email");
                        return BadRequest(errorResponse);
                    }

                    // Check the correct Employee Name
                    var employeeName = await _context.EmployeeRegister.FirstOrDefaultAsync(n => n.EmployeeName == attendance.EmployeeName);
                    if (employeeName == null)
                    {
                        var errorResponse = _errorResponseService.CreateErrorResponse(400, "Please select valid Employee Name");
                        return BadRequest(errorResponse);
                    }

                    // Check if attendance date is in the past or present
                    if (attendance.AttendanceDate.Date <= DateTime.Now.Date)
                    {
                        var errorResponse = _errorResponseService.CreateErrorResponse(400, "Attendance date cannot be in the past");
                        return BadRequest(errorResponse);
                    }

                    _context.Attendance.Add(attendance);
                    await _context.SaveChangesAsync();

                    //Success Response
                    var response = new
                    {
                        Status = 200,
                        Message = "Attendance Marked Successfully",
                        Data = attendance
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

        //POST: Mark Attendance by Filtering Department
        [HttpPost]
        [Route("MarkAttendanceForDepartment")]
        public async Task<ActionResult<IEnumerable<Attendance>>> MarkAttendanceForDepartment([FromForm] int departmentId, [FromForm] DateTime attendanceDate, [FromForm] AttendanceStatus attendanceStatus)
        {
            try
            {
                if (departmentId <= 0)
                {
                    var errorResponse = _errorResponseService.CreateErrorResponse(400, "Please provide a valid DepartmentId");
                    return BadRequest(errorResponse);
                }

                var employeeRegisterList = await _context.EmployeeRegister
                    .Where(e => e.DepartmentId == departmentId)
                    .Select(e => new
                    {
                        EmployeeId = e.EmployeeId,
                        EmployeeEmail = e.EmployeeEmail,
                        EmployeeName = e.EmployeeName
                    })
                    .ToListAsync();

                if (employeeRegisterList.Count == 0)
                {
                    var errorResponse = _errorResponseService.CreateErrorResponse(404, $"No employees found for DepartmentId: {departmentId}");
                    return NotFound(errorResponse);
                }

                var attendanceList = new List<Attendance>();

                foreach (var employee in employeeRegisterList)
                {
                    var attendance = new Attendance
                    {
                        DepartmentId = departmentId,
                        AttendanceDate = attendanceDate,
                        EmployeeId = employee.EmployeeId,
                        EmployeeEmail = employee.EmployeeEmail,
                        EmployeeName = employee.EmployeeName,
                        AttendanceStatus = attendanceStatus,
                        CreatedOn = DateTime.Now
                    };

                    attendanceList.Add(attendance);
                }

                // Insert the attendance records into the database
                _context.Attendance.AddRange(attendanceList);
                await _context.SaveChangesAsync();

                var response = new
                {
                    Status = 200,
                    Message = "Attendance records inserted successfully",
                    Data = attendanceList
                };

                return Ok(response);
            }
            catch (Exception)
            {
                var errorResponse = _errorResponseService.CreateErrorResponse(500, "Internal Server Error");
                return BadRequest(errorResponse);
            }
        }

        //GET: GetList of Attendance
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Attendance>>> GetAttendanceList()
        {
            try
            {
                var attendanceList = await _context.Attendance.ToListAsync();
                return Ok(attendanceList);
            }
            catch (Exception)
            {
                var errorResponse = _errorResponseService.CreateErrorResponse(500, "Internal Server Error");
                return StatusCode(500, errorResponse);
            }
        }

        //GET: Filter of Attendance by DepartmentId and Date ~~
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Attendance>>> GetFilteredAttendanceList([FromQuery] int departmentId, [FromQuery] DateTime attendanceDate)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var filteredAttendanceList = await _context.Attendance
                    .Where(a => a.DepartmentId == departmentId && a.AttendanceDate.Date == attendanceDate.Date)
                    .ToListAsync();

                    if (filteredAttendanceList == null || !filteredAttendanceList.Any())
                    {
                        var errorResponse = _errorResponseService.CreateErrorResponse(400, "Department Id and Attendance Date is invalid");
                        return BadRequest(errorResponse);
                    }

                    //Success Response
                    var response = new
                    {
                        Status = 200,
                        Message = "Data Found Successfully",
                        Data = filteredAttendanceList
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


        //GET: Attendance by EmployeeEmail and Month of AttendanceDate
        [HttpGet("GetAttendanceListByMonth")]
        public async Task<ActionResult<IEnumerable<object>>> GetAttendanceListByMonth([FromQuery] string employeeEmail, [FromQuery] int month)
        {
            try
            {
                if (string.IsNullOrEmpty(employeeEmail))
                {
                    var errorResponse = _errorResponseService.CreateErrorResponse(400, "Please provide a valid EmployeeEmail");
                    return BadRequest(errorResponse);
                }

                var attendanceList = await _context.Attendance
                    .Where(a => a.EmployeeEmail.ToLower() == employeeEmail.ToLower() && a.AttendanceDate.Month == month)
                    .Select(a => new
                    {
                        AttendanceDate = a.AttendanceDate,
                        AttendanceStatus = a.AttendanceStatus
                    })
                    .ToListAsync();

                if (attendanceList == null || !attendanceList.Any())
                {
                    var errorResponse = _errorResponseService.CreateErrorResponse(404, "No attendance records found for the specified EmployeeEmail and month");
                    return NotFound(errorResponse);
                }

                //Success Response
                var response = new
                {
                    Status = 200,
                    Message = "Data Found Successfully",
                    Data = attendanceList
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
