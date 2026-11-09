
using EmployeeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class EmployeeController : Controller
    {
        private readonly EmployeeProfileService _profileService;

        public EmployeeController(EmployeeProfileService profileService)
        {
            _profileService = profileService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var employee = await _profileService.SyncEmployeeAsync(User);

            return View(employee);
        }
    }
}
