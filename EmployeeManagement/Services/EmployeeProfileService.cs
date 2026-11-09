
using System.Security.Claims;
using EmployeeManagement.Data;
using EmployeeManagement.Models;
using Microsoft.EntityFrameworkCore;
namespace EmployeeManagement.Services
{
    public class EmployeeProfileService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeProfileService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Employee> SyncEmployeeAsync(ClaimsPrincipal user)
        {
            // Get the employee's unique Microsoft Entra Object ID
            var entraObjectId =
                user.FindFirst("oid")?.Value ??
                user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            // Get the employee's name
            var fullName =
                user.FindFirst("name")?.Value ??
                user.Identity?.Name;

            // Get the employee's email or sign-in username
            var email =
                user.FindFirst("preferred_username")?.Value ??
                user.FindFirst(ClaimTypes.Email)?.Value ??
                user.FindFirst("email")?.Value;

            if (string.IsNullOrWhiteSpace(entraObjectId))
            {
                throw new InvalidOperationException(
                    "Microsoft Entra Object ID was not found.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new InvalidOperationException(
                    "Employee email was not found in the claims.");
            }

            fullName = string.IsNullOrWhiteSpace(fullName)
                ? email
                : fullName;

            // Check whether this employee already exists
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EntraObjectId == entraObjectId);

            if (employee == null)
            {
                // Create a new employee record
                employee = new Employee
                {
                    EntraObjectId = entraObjectId,
                    FullName = fullName,
                    Email = email,
                    IsActive = true
                };

                _context.Employees.Add(employee);
            }
            else
            {
                // Update identity information only
                employee.FullName = fullName;
                employee.Email = email;
            }

            await _context.SaveChangesAsync();

            return employee;
        }
    }
}
