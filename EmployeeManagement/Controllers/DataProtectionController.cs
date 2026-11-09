using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DataProtectionController : Controller
    {
        private readonly IDataProtector _protector;

        public DataProtectionController(
            IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector(
                "EmployeeManagement.Demo.v1");
        }

        public IActionResult Demo()
        {
            string originalData = "Employee Salary: 50000";

            // Protect the original data
            string protectedData =
                _protector.Protect(originalData);

            // Recover the original data
            string decryptedData =
                _protector.Unprotect(protectedData);

            return Content(
                $"Original Data: {originalData}\n\n" +
                $"Protected Data: {protectedData}\n\n" +
                $"Decrypted Data: {decryptedData}");
        }
    }
}