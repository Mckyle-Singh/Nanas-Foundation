using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Nanas_Foundation.Data;
using Nanas_Foundation.Models;

namespace Nanas_Foundation.Controllers
{
    public class VolunteersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _activeUser;

        public VolunteersController(ApplicationDbContext context, UserManager<IdentityUser> activeUser)
        {
            _context = context;
            _activeUser = activeUser;
        }

        [HttpGet]
        [ActionName("VolunteerForEvent")]
        public async Task<IActionResult> ShowVolunteerForm()
        {
            var today = DateTime.Today;

            var events = await _context.Events
                .Where(e => e.Date >= today) // Only include future or current events
                .OrderBy(e => e.Date)        // Optional: sort chronologically
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.Title} ({e.Date:MMM d, yyyy})"
                })
                .ToListAsync();

            ViewBag.Events = events;
            return View("VolunteerForEvent");

        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> VolunteerForEvent(Guid eventId)
        {
            var userId = _activeUser.GetUserId(User);

            var alreadyVolunteered = await _context.Volunteers
                .AnyAsync(v => v.EventId == eventId && v.UserId == userId);

            if (alreadyVolunteered)
                return RedirectToAction("Index", "Event");

            var volunteer = new Volunteer
            {
                EventId = eventId,
                UserId = userId
            };

            _context.Volunteers.Add(volunteer);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Event");
        }

    }
}
