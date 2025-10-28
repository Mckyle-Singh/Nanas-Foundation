using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nanas_Foundation.Areas.Admin.ViewModels;
using Nanas_Foundation.Data;
using System.Globalization;

namespace Nanas_Foundation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var currentYear = DateTime.UtcNow.Year;

            var monthlyTotals = _context.Donations
                .Where(d => d.DonationDate.Year == currentYear)
                .GroupBy(d => d.DonationDate.Month)
                .Select(g => new MonthlyDonationViewModel
                {
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(g.Key),
                    TotalAmount = g.Sum(d => d.Amount)
                })
                .AsEnumerable()
                .OrderBy(m => DateTime.ParseExact(m.MonthName, "MMM", CultureInfo.InvariantCulture).Month)
                .ToList();

            var totalDonations = monthlyTotals.Sum(m => m.TotalAmount);
            ViewBag.TotalDonations = totalDonations;

            // Events planned for the year
            var eventsPlanned = _context.Events
                .Where(e => e.Date.Year == currentYear)
                .Count();
            ViewBag.EventsPlanned = eventsPlanned;

            // Events grouped by month (for pie chart)
            var eventsByMonth = _context.Events
                .Where(e => e.Date.Year == currentYear)
                .GroupBy(e => e.Date.Month)
                .Select(g => new
                {
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(g.Key),
                    Count = g.Count()
                })
                .ToList()
                .OrderBy(e => DateTime.ParseExact(e.MonthName, "MMM", CultureInfo.InvariantCulture).Month)
                .ToList();

            ViewBag.EventsByMonth = eventsByMonth;


            // Total volunteers signed up
            var totalVolunteers = _context.Volunteers.Count();
            ViewBag.TotalVolunteers = totalVolunteers;

            return View(monthlyTotals);

        }
    }
}
