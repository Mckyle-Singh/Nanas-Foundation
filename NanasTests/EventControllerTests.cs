using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nanas_Foundation.Controllers;
using Nanas_Foundation.Data;
using Nanas_Foundation.Models;
using System;
using System.Linq;
using X.PagedList;
using Xunit;

namespace Nanas_Foundation.Tests.Controllers
{
    public class EventControllerTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB per test
                .Options;

            var context = new ApplicationDbContext(options);
            return context;
        }

        [Fact]
        public void Index_ReturnsViewWithEvents()
        {
            // Arrange
            var context = GetInMemoryContext();
            context.Events.Add(new Event
            {
                Id = Guid.NewGuid(),
                Title = "Charity Run",
                Description = "Community charity run for local causes",
                Location = "City Stadium",
                Date = DateTime.Now
            });
            context.SaveChanges();

            var controller = new EventController(context);

            // Act
            var result = controller.Index("", 1) as ViewResult; // Include required parameters
            var model = result?.Model as IPagedList<Event>;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Single(model);
            Assert.Equal("Charity Run", model.First().Title);
        }

        [Fact]
        public void Create_ValidEvent_RedirectsToIndex()
        {
            var context = GetInMemoryContext();
            var controller = new EventController(context);

            var evt = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Fundraiser",
                Description = "Annual fundraising gala",
                Location = "Community Hall",
                Date = DateTime.Now
            };

            var result = controller.Create(evt) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Single(context.Events);
        }

        [Fact]
        public void Edit_InvalidId_ReturnsBadRequest()
        {
            var context = GetInMemoryContext();
            var controller = new EventController(context);
            var evt = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Test Event",
                Description = "Testing event editing",
                Location = "Virtual"
            };

            var result = controller.Edit(Guid.NewGuid(), evt);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public void Delete_ValidId_RemovesEvent()
        {
            var context = GetInMemoryContext();
            var evt = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Charity Gala",
                Description = "Evening fundraising event",
                Location = "City Center"
            };
            context.Events.Add(evt);
            context.SaveChanges();

            var controller = new EventController(context);

            var result = controller.Delete(evt.Id) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Empty(context.Events);
        }
    }
}
