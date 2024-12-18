using Microsoft.AspNetCore.Mvc;
using RBPWEB.Models;
using System.Diagnostics;

namespace RBPWEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string? message)
        {
            
            ViewBag.Message = message;
            return View();
        }

        
        [HttpPost]
        public IActionResult ShowMessage()
        {
            
            return RedirectToAction("Index", new { message = "��� ���������� ���������� ASP.NET" });
        }
        public IActionResult LAB8_3()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DisplayGreeting(string firstName, string lastName)
        {

            ViewBag.Greeting = $"����� ����������, {firstName} {lastName}!";
            return View("LAB8_3");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
