using EmployeeManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EmployeeManagement.Controllers
{
    public class HomeController : Controller
    {
        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            return Content("Welcome, Administrator! You can manage employees.");
        }

        [Authorize(Roles = "Employee")]
        public IActionResult EmployeeDashboard()
        {
            //return Content("Welcome, Employee! You can view your information.");
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }
        [Authorize(Policy = "CanManageEmployees")]
        public IActionResult ManageEmployees()
        {
            return Content("You are authorized to manage employees.");
        }

        [Authorize(Policy = "CanViewEmployees")]
        public IActionResult ViewEmployees()
        {
            return Content("You are authorized to view employees.");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
