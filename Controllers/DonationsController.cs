using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nanas_Foundation.Data;
using Nanas_Foundation.Models;

namespace Nanas_Foundation.Controllers
{
    [Authorize]
    public class DonationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DonationsController> _logger;

        public DonationsController(ApplicationDbContext context, ILogger<DonationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Ensure the user is logged in (Authorize handles this, but extra safety)
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                TempData["LoginMessage"] = "Please log in to make a donation.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Banks = new[] { "FNB", "Standard Bank", "ABSA", "Nedbank" };
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Donation model)
        {
            ViewBag.Banks = new[] { "FNB", "Standard Bank", "ABSA", "Nedbank" };

            if (!User.Identity?.IsAuthenticated ?? true)
            {
                TempData["LoginMessage"] = "Please log in to make a donation.";
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Donation Create: ModelState invalid.");
                TempData["DonationError"] = "Please correct the errors and try again.";
                return View(model);
            }

            model.DonationDate = DateTime.UtcNow;

            try
            {
                _context.Donations.Add(model);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Donation saved. DonationID={DonationID}, Amount={Amount}",
                    model.DonationID, model.Amount);

                return RedirectToAction(nameof(ThankYou));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving donation.");
                TempData["DonationError"] = "Donation failed to submit. Please try again later.";
                return View(model);
            }
        }

        public IActionResult ThankYou() => View();
    }
}
