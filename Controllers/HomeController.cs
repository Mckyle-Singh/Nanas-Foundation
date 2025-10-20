using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Nanas_Foundation.Models;

namespace Nanas_Foundation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Mission()
        {
            return View();
        }

        public IActionResult Resources()
        {
            return View();
        }

        public IActionResult GetInvolved()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Team()
        {
            return View();
        }

        public IActionResult Blog()
        {
            return View();
        }

        public IActionResult FAQ()
        {
            return View();
        }

        public IActionResult Help()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
