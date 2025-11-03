using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Moq;
using Nanas_Foundation.Controllers;
using Nanas_Foundation.Data;
using Nanas_Foundation.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Nanas_Foundation.Tests.Controllers
{
    public class VolunteersControllerTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        private Mock<UserManager<IdentityUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            return new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task ShowVolunteerForm_ReturnsViewWithEventsList()
        {
            // Arrange
            var context = GetInMemoryContext();
            context.Events.Add(new Event
            {
                Id = Guid.NewGuid(),
                Title = "Tree Planting",
                Description = "Environmental community service",
                Location = "City Park",
                Date = DateTime.Today.AddDays(1)
            });
            context.SaveChanges();

            var mockUserManager = GetMockUserManager();
            var controller = new VolunteersController(context, mockUserManager.Object);

            // Act
            var result = await controller.ShowVolunteerForm() as ViewResult;
            var eventsList = result?.ViewData["Events"] ?? controller.ViewBag.Events as List<SelectListItem>;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("VolunteerForEvent", result.ViewName);
            Assert.NotNull(eventsList);
        }

        [Fact]
        public async Task VolunteerForEvent_NewVolunteer_AddsAndReturnsView()
        {
            // Arrange
            var context = GetInMemoryContext();
            var evt = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Beach Cleanup",
                Description = "Cleaning up the local beach",
                Location = "Main Beach",
                Date = DateTime.Today.AddDays(1)
            };
            context.Events.Add(evt);
            await context.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user123");

            var controller = new VolunteersController(context, mockUserManager.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "user123")
                    }, "mock"))
                }
            };

            // Act
            var result = await controller.VolunteerForEvent(evt.Id) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("VolunteerForEvent", result.ViewName);
            Assert.Single(context.Volunteers);
            Assert.Equal("Thank you for volunteering!", result.ViewData["Message"] ?? controller.ViewBag.Message);
        }

        [Fact]
        public async Task VolunteerForEvent_AlreadyVolunteered_DoesNotDuplicate()
        {
            // Arrange
            var context = GetInMemoryContext();
            var evt = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Charity Walk",
                Description = "Fundraising walk event",
                Location = "Town Square",
                Date = DateTime.Today.AddDays(1)
            };
            var volunteer = new Volunteer { EventId = evt.Id, UserId = "user123" };

            context.Events.Add(evt);
            context.Volunteers.Add(volunteer);
            await context.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user123");

            var controller = new VolunteersController(context, mockUserManager.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "user123")
                    }, "mock"))
                }
            };

            // Act
            var result = await controller.VolunteerForEvent(evt.Id) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("VolunteerForEvent", result.ViewName);
            Assert.Single(context.Volunteers); // no duplicates
            Assert.Equal("You’ve already volunteered for this event.", result.ViewData["Message"] ?? controller.ViewBag.Message);
        }
    }
}