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
    public class DepartmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ErrorResponseService _errorResponseService;

        public DepartmentController(ApplicationDbContext context, ErrorResponseService errorResponseService)
        {
            _context = context;
            _errorResponseService = errorResponseService;
        }

        //GET: api/Department
        [HttpGet]
        public async Task<ActionResult<Department>> getDepartmentList()
        {
            try
            {
                var registeredDepartment = _context.Department.ToList();
                //Success Message
                var response = new
                {
                    Status = 200,
                    Message = "Department List Fetched",
                    Data = registeredDepartment.ToList()
                };
                return Created("", response);
            }
            catch (Exception)
            {
                var errorResponse = _errorResponseService.CreateErrorResponse(500, "Internal Server Error");
                return StatusCode(500, errorResponse);
            }
        }

        //GET: api/Department/{id}
        [HttpGet("{departmentId}")]
        public async Task<ActionResult<Department>> getDepartmentNameById(int departmentId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var department = await _context.Department
                        .Where(d => d.DepartmentId == departmentId)
                        .Select(d => d.DepartmentName)
                        .FirstOrDefaultAsync();

                    if (department == null)
                    {
                        var errorResponse = _errorResponseService.CreateErrorResponse(400, "Please enter valid Department Id");
                        return BadRequest(errorResponse);
                    }

                    var response = new
                    {
                        Status = 200,
                        Message = "Found Department Successfully",
                        Data = department
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

        //POST: api/Department
        [HttpPost]
        public async Task<ActionResult<Department>> DepartmentRegister(Department department)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //Check if DepartmentName is empty
                    if (string.IsNullOrWhiteSpace(department.DepartmentName))
                    {
                        var errorResponse = _errorResponseService.CreateErrorResponse(400, "Department Name is Mandatory");
                        return BadRequest(errorResponse);
                    }

                    //Check if Department already exist
                    var existDepartment = await _context.Department.FirstOrDefaultAsync(d => d.DepartmentName == department.DepartmentName);
                    if (existDepartment != null)
                    {
                        var errorResponse = _errorResponseService.CreateErrorResponse(400, "Department Already Exist!");
                        return BadRequest(errorResponse);
                    }

                    _context.Department.Add(department);
                    await _context.SaveChangesAsync();

                    //Success Message
                    var response = new
                    {
                        Status = 200,
                        Message = "Department Saved Successfully",
                        Data = department
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

        //DELETE/Department/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<Department>> deleteDepartmentById(int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var department = await _context.Department.FindAsync(id);

                    if (department == null)
                    {
                        var errorResponse = _errorResponseService.CreateErrorResponse(400, "Please enter valid department");
                        return BadRequest(errorResponse);
                    }

                    _context.Department.Remove(department);
                    await _context.SaveChangesAsync();

                    var response = new
                    {
                        Status = 200,
                        Message = "Department Deleted Successful",
                        Data = department
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
