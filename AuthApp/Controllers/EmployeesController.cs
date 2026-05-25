using AuthApp.Data;
using AuthApp.DTOs;
using AuthApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _context.Employees
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound(new { message = "Employee not found" });
            return Ok(employee);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exists = await _context.Employees
                .AnyAsync(e => e.Email == dto.Email);
            if (exists)
                return BadRequest(new { message = "Employee with this email already exists" });

            var employee = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                Department = dto.Department,
                Position = dto.Position,
                Salary = dto.Salary,
                JoiningDate = dto.JoiningDate
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById),
                new { id = employee.Id }, employee);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id,
            [FromBody] UpdateEmployeeDTO dto)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound(new { message = "Employee not found" });

            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.Email = dto.Email;
            employee.Phone = dto.Phone;
            employee.Department = dto.Department;
            employee.Position = dto.Position;
            employee.Salary = dto.Salary;
            employee.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return Ok(employee);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound(new { message = "Employee not found" });

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Employee deleted successfully" });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var employees = await _context.Employees
                .Where(e => e.FirstName.Contains(query) ||
                            e.LastName.Contains(query) ||
                            e.Email.Contains(query) ||
                            e.Department.Contains(query))
                .ToListAsync();
            return Ok(employees);
        }
    }
}