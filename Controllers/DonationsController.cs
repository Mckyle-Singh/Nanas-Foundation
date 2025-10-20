using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nanas_Foundation.Data;
using Nanas_Foundation.Models;
using Stripe;
using Stripe.Checkout;

namespace Nanas_Foundation.Controllers
{
    // NOTE: no [Authorize] attribute here — we check authentication manually
    public class DonationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DonationsController> _logger;
        private readonly string _successUrl;
        private readonly string _cancelUrl;

        public DonationsController(ApplicationDbContext context, ILogger<DonationsController> logger)
        {
            _context = context;
            _logger = logger;

            _successUrl = Environment.GetEnvironmentVariable("STRIPE_SUCCESS_URL")
                          ?? throw new InvalidOperationException("STRIPE_SUCCESS_URL not configured.");
            _cancelUrl = Environment.GetEnvironmentVariable("STRIPE_CANCEL_URL")
                         ?? throw new InvalidOperationException("STRIPE_CANCEL_URL not configured.");
            // StripeConfiguration.ApiKey should be set at startup (Program.cs)
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Manual guard: redirect to Home/Index with message if NOT logged in
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

            try
            {
                // Convert decimal amount to cents (Stripe uses smallest currency unit)
                var amountInCents = Convert.ToInt64(Math.Round(model.Amount * 100M));

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    Mode = "payment",
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = amountInCents,
                                Currency = "zar",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "Nanas Foundation Donation",
                                    Description = $"Bank: {model.Bank}"
                                }
                            },
                            Quantity = 1
                        }
                    },
                    SuccessUrl = _successUrl + "?session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = _cancelUrl,
                    Metadata = new Dictionary<string, string>
                    {
                        { "bank", model.Bank ?? "N/A" }
                    }
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                if (!string.IsNullOrEmpty(session.Url))
                    return Redirect(session.Url);

                // Fallback (rare): direct to checkout by session id
                return Redirect($"https://checkout.stripe.com/pay/{session.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Stripe Checkout session.");
                TempData["DonationError"] = "Payment initialization failed. Please try again.";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Success(string session_id)
        {
            // only allow logged-in users to view/save donation; otherwise send to home
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                TempData["LoginMessage"] = "Please log in to view your donation confirmation.";
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrEmpty(session_id))
            {
                TempData["DonationError"] = "Missing session id.";
                return RedirectToAction("Create");
            }

            try
            {
                var sessionService = new SessionService();
                var session = await sessionService.GetAsync(session_id);

                if (!string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Stripe session {SessionId} payment status: {Status}", session_id, session.PaymentStatus);
                    TempData["DonationError"] = "Payment not completed.";
                    return RedirectToAction("Create");
                }

                // Safely determine amount in cents
                long? amountCents = session.AmountTotal;
                if (!amountCents.HasValue || amountCents.Value <= 0)
                {
                    var lineItemService = new SessionLineItemService();
                    var lineItems = await lineItemService.ListAsync(session.Id, new SessionLineItemListOptions { Limit = 1 });
                    var first = lineItems?.Data?.FirstOrDefault();
                    if (first != null)
                    {
                        long? fromPrice = null;
                        long? fromSubtotal = null;
                        long? fromAmountTotal = null;
                        try { fromPrice = first.Price?.UnitAmount; } catch { fromPrice = null; }
                        try { fromSubtotal = first.AmountSubtotal; } catch { fromSubtotal = null; }
                        try { fromAmountTotal = first.AmountTotal; } catch { fromAmountTotal = null; }

                        amountCents = fromPrice ?? fromSubtotal ?? fromAmountTotal;
                    }
                }

                if (!amountCents.HasValue || amountCents.Value <= 0)
                {
                    _logger.LogError("Unable to determine amount for Stripe session {SessionId}", session_id);
                    TempData["DonationError"] = "Could not determine donation amount from Stripe session.";
                    return RedirectToAction("Create");
                }

                decimal amount = amountCents.Value / 100m;

                // bank from metadata if provided
                string bank = "Stripe";
                try
                {
                    if (session.Metadata != null && session.Metadata.ContainsKey("bank"))
                        bank = session.Metadata["bank"] ?? "Stripe";
                }
                catch { /* ignore metadata errors */ }

                var donation = new Donation
                {
                    Amount = amount,
                    Bank = bank,
                    DonationDate = DateTime.UtcNow
                };

                _context.Donations.Add(donation);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Saved donation from Stripe session {SessionId} amount {Amount}", session_id, amount);

                return RedirectToAction(nameof(ThankYou));
            }
            catch (StripeException se)
            {
                _logger.LogError(se, "Stripe exception when handling Success for session {SessionId}", session_id);
                TempData["DonationError"] = "Payment verification failed. Please contact support.";
                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception when handling Success for session {SessionId}", session_id);
                TempData["DonationError"] = "An error occurred while saving your donation. Please try again.";
                return RedirectToAction("Create");
            }
        }

        public IActionResult ThankYou() => View();
    }
}
