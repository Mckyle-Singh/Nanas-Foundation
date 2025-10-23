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

            return View(monthlyTotals);

        }
    }
}
