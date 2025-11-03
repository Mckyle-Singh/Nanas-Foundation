using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Nanas_Foundation.Controllers;
using Nanas_Foundation.Data;
using Nanas_Foundation.Models;
using Stripe;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Nanas_Foundation.Tests.Controllers
{
    public class DonationsControllerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public DonationsControllerTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private DonationsController CreateController(bool isAuthenticated = true)
        {
            Environment.SetEnvironmentVariable("STRIPE_SUCCESS_URL", "https://success");
            Environment.SetEnvironmentVariable("STRIPE_CANCEL_URL", "https://cancel");

            var context = new ApplicationDbContext(_options);
            var logger = new Mock<ILogger<DonationsController>>();

            var controller = new DonationsController(context, logger.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(
                isAuthenticated ? new[] { new Claim(ClaimTypes.Name, "TestUser") } : new Claim[] { },
                isAuthenticated ? "mock" : ""
            ));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            controller.TempData = new TempDataDictionary(controller.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());

            return controller;
        }


        // GET: Create

        [Fact]
        public void Create_Get_RedirectsIfUnauthenticated()
        {
            var controller = CreateController(isAuthenticated: false);

            var result = controller.Create(100m) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Home", result.ControllerName);
            Assert.Equal("Please log in to make a donation.", controller.TempData["LoginMessage"]);
        }

        [Fact]
        public void Create_Get_ReturnsViewIfAuthenticated()
        {
            var controller = CreateController(isAuthenticated: true);

            var result = controller.Create(200m) as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<Donation>(result.Model);
            var model = (Donation)result.Model;
            Assert.Equal(200m, model.Amount);
        }


        // POST: Create

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsView()
        {
            var controller = CreateController();
            controller.ModelState.AddModelError("Amount", "Required");

            var result = await controller.Create(new Donation()) as ViewResult;

            Assert.NotNull(result);
            Assert.True(controller.TempData.ContainsKey("DonationError"));
        }

        [Fact]
        public async Task Create_Post_RedirectsIfUnauthenticated()
        {
            var controller = CreateController(isAuthenticated: false);

            var result = await controller.Create(new Donation { Amount = 100, Bank = "FNB" }) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Home", result.ControllerName);
            Assert.Equal("Please log in to make a donation.", controller.TempData["LoginMessage"]);
        }

        [Fact]
        public async Task Create_Post_StripeException_ReturnsView()
        {
            var controller = CreateController();

            // Simulate invalid environment (missing Stripe API key from anza)
            Environment.SetEnvironmentVariable("STRIPE_SUCCESS_URL", null);

            // Wrap in try-catch since constructor will throw
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                var c = new DonationsController(new ApplicationDbContext(_options), Mock.Of<ILogger<DonationsController>>());
                await c.Create(new Donation { Amount = 100, Bank = "FNB" });
            });
        }


        // GET: Success

        [Fact]
        public async Task Success_RedirectsIfUnauthenticated()
        {
            var controller = CreateController(isAuthenticated: false);

            var result = await controller.Success("abc123") as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Home", result.ControllerName);
        }

        [Fact]
        public async Task Success_MissingSessionId_RedirectsToCreate()
        {
            var controller = CreateController();

            var result = await controller.Success(null) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Create", result.ActionName);
        }

        [Fact]
        public void ThankYou_ReturnsView()
        {
            var controller = CreateController();

            var result = controller.ThankYou() as ViewResult;

            Assert.NotNull(result);
        }
    }
}
