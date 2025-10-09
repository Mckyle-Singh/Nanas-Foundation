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
            var events = _context.Events.ToList();
            return View(events);
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

        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var evt = _context.Events.Find(id);
            if (evt == null) return NotFound();
            return View(evt);
        }

        [HttpPost]
        public IActionResult Edit(Guid id, Event updatedEvent)
        {
            if (id != updatedEvent.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                _context.Events.Update(updatedEvent);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(updatedEvent);
        }

        [HttpPost]
        public IActionResult Delete(Guid id)
        {
            var evt = _context.Events.Find(id);
            if (evt == null) return NotFound();

            _context.Events.Remove(evt);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetDetailsJson(Guid id)
        {
            var evt = await _context.Events
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    name = e.Title,
                    description = e.Description,
                    date = e.Date.ToString("yyyy-MM-dd"),
                    location = e.Location
                })
                .FirstOrDefaultAsync();

            if (evt == null)
                return NotFound();

            return Json(evt);
        }

        
    }
}
