
using EmployeeManagement.Data;
using EmployeeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using EmployeeManagement.Models;


namespace EmployeeManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AesGcmEncryptionService _encryptionService;
        public AdminController(ApplicationDbContext context, AesGcmEncryptionService encryptionService)
        {
            _context = context;
            _encryptionService = encryptionService;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        /*  public async Task<IActionResult> Employees()
          {
              var employees = await _context.Employees
                  .AsNoTracking()
                  .OrderBy(e => e.FullName)
                  .ToListAsync();

              return View(employees);
          }*/
        public async Task<IActionResult> Employees(string? search)
        {
            var query = _context.Employees
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e =>
                    e.FullName.Contains(search) ||
                    e.Email.Contains(search) ||
                    (e.Department != null &&
                     e.Department.Contains(search)));
            }

            var employees = await query
                .OrderBy(e => e.FullName)
                .ToListAsync();

            ViewBag.Search = search;

            return View(employees);
        }

        [HttpGet]
        public IActionResult AddEmployee()
        {
            return View(new EmployeeManagement.Models.AddEmployeeViewModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmployee(
            EmployeeManagement.Models.AddEmployeeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Start a database transaction so both saves succeed or fail together.
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // Step 1: Create the employee with ordinary details.
                var employee = new EmployeeManagement.Models.Employee
                {
                    EntraObjectId = "PENDING-" + Guid.NewGuid(),
                    FullName = model.FullName,
                    Email = model.Email,
                    Department = model.Department,
                    Designation = model.Designation,
                    JoiningDate = model.JoiningDate,
                    IsActive = true
                };

                _context.Employees.Add(employee);

                // Step 2: Save to generate EmployeeId.
                await _context.SaveChangesAsync();

                // Step 3: Encrypt confidential notes if provided.
                if (!string.IsNullOrWhiteSpace(model.ConfidentialNotes))
                {
                    byte[] associatedData = Encoding.UTF8.GetBytes(
                        $"Employee:{employee.EmployeeId}:ConfidentialNotes");

                    var encrypted = _encryptionService.Encrypt(
                        model.ConfidentialNotes,
                        associatedData);

                    // Step 4: Store only encrypted values in the entity.
                    employee.ConfidentialNotesCiphertext = encrypted.Ciphertext;
                    employee.ConfidentialNotesNonce = encrypted.Nonce;
                    employee.ConfidentialNotesTag = encrypted.Tag;
                    employee.ConfidentialNotesKeyVersion = encrypted.KeyVersion;

                    // Step 5: Save encrypted values.
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return RedirectToAction(nameof(Employees));
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditEmployee(int id)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            string? confidentialNotes = null;

            if (employee.ConfidentialNotesCiphertext != null)
            {
                byte[] associatedData = Encoding.UTF8.GetBytes(
                    $"Employee:{employee.EmployeeId}:ConfidentialNotes");

                confidentialNotes = _encryptionService.Decrypt(
                    employee.ConfidentialNotesCiphertext,
                    employee.ConfidentialNotesNonce,
                    employee.ConfidentialNotesTag,
                    associatedData);
            }

            var model = new EmployeeManagement.Models.EmployeeEditViewModel
            {
                EmployeeId = employee.EmployeeId,
                FullName = employee.FullName,
                Email = employee.Email,
                Department = employee.Department,
                Designation = employee.Designation,
                JoiningDate = employee.JoiningDate,
                IsActive = employee.IsActive,
                ConfidentialNotes = confidentialNotes
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmployee(
            EmployeeManagement.Models.EmployeeEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Step 1: Retrieve the existing employee.
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == model.EmployeeId);

            if (employee == null)
            {
                return NotFound();
            }

            // Step 2: Update ordinary employee details.
            employee.FullName = model.FullName;
            employee.Email = model.Email;
            employee.Department = model.Department;
            employee.Designation = model.Designation;
            employee.JoiningDate = model.JoiningDate;
            employee.IsActive = model.IsActive;

            // Step 3: Encrypt replacement notes if Admin entered them.
            if (!string.IsNullOrWhiteSpace(model.ConfidentialNotes))
            {
                byte[] associatedData = Encoding.UTF8.GetBytes(
                    $"Employee:{employee.EmployeeId}:ConfidentialNotes");

                var encrypted = _encryptionService.Encrypt(
                    model.ConfidentialNotes,
                    associatedData);

                employee.ConfidentialNotesCiphertext = encrypted.Ciphertext;
                employee.ConfidentialNotesNonce = encrypted.Nonce;
                employee.ConfidentialNotesTag = encrypted.Tag;
                employee.ConfidentialNotesKeyVersion = encrypted.KeyVersion;
            }

            // Step 4: Save all changes.
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Employees));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
                return NotFound();

            return View(employee);
        }

        [HttpPost]
        [ActionName("DeleteEmployee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEmployeeConfirmed(int id)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
                return NotFound();

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Employees));
        }



    }
}
