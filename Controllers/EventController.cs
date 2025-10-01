using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nanas_Foundation.Data;
using Nanas_Foundation.Models;

namespace Nanas_Foundation.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;
        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Event evt)
        {
            if (ModelState.IsValid)
            {
                _context.Events.Add(evt);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(evt);
        }

    }
}
