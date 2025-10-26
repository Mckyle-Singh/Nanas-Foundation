using Microsoft.AspNetCore.Mvc;
using Nanas_Foundation.Data;
using Nanas_Foundation.Models;
using System.Diagnostics;

namespace Nanas_Foundation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;


        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var latestEvent = _context.Events
            .Where(e => e.Date >= DateTime.Today)
            .OrderBy(e => e.Date)
            .FirstOrDefault();

            return View(latestEvent);

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

        public IActionResult NewsEvents()
        {
            var upcomingEvents = _context.Events
            .Where(e => e.Date >= DateTime.Today)
            .OrderBy(e => e.Date)
            .Take(4)
            .ToList();

            var recentBlogs = _context.BlogPosts
            .OrderByDescending(b => b.CreatedAt)
            .Take(4)
            .ToList();

            return View(Tuple.Create(upcomingEvents, recentBlogs));
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
