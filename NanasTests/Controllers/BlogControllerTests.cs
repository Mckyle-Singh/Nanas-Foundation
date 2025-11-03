using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Nanas_Foundation.Controllers;
using Nanas_Foundation.Data;
using Nanas_Foundation.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Nanas_Foundation.Tests.Controllers
{
    public class BlogControllerTests
    {
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly ApplicationDbContext _context;

        public BlogControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "BlogControllerTestDB")
                .Options;
            _context = new ApplicationDbContext(options);

            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockEnv.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
        }

        [Fact]
        public void Index_ReturnsViewWithAllBlogs_WhenNoSearchProvided()
        {
            var context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

            context.BlogPosts.AddRange(
                new BlogPost { Id = Guid.NewGuid(), Title = "ToDelete", AuthorName = "Test Author", CreatedAt = DateTime.UtcNow, AuthorEmail = "test@example.com", WebsiteLink = "https://example.com" },
                new BlogPost { Id = Guid.NewGuid(), Title = "ToDelete", AuthorName = "Test Author", CreatedAt = DateTime.UtcNow, AuthorEmail = "test@example.com", WebsiteLink = "https://example.com" }
            );
            context.SaveChanges();

            var controller = new BlogController(context, _mockEnv.Object);

            var result = controller.Index(null) as ViewResult;

            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<IEnumerable<BlogPost>>(result.Model);
            Assert.Equal(2, model.Count());
        }


        [Fact]
        public void Index_FiltersResults_WhenSearchTermProvided()

        {
            // Arrange
            _context.BlogPosts.AddRange(
                new BlogPost { Id = Guid.NewGuid(), Title = "AI Blog", AuthorName = "Admin", CreatedAt = DateTime.UtcNow, AuthorEmail = "test@example.com", WebsiteLink = "https://example.com", });

            new BlogPost { Id = Guid.NewGuid(), Title = "Cooking Tips", AuthorName = "Chef", CreatedAt = DateTime.UtcNow, AuthorEmail = "test@example.com", WebsiteLink = "https://example.com", };
            _context.SaveChanges();

            var controller = new BlogController(_context, _mockEnv.Object);

            // Act
            var result = controller.Index("AI") as ViewResult;

            // Assert
            var model = Assert.IsAssignableFrom<IEnumerable<BlogPost>>(result.Model);
            Assert.Single(model);
            Assert.Equal("AI Blog", model.First().Title);
        }

        [Fact]
        public async Task Create_ReturnsRedirect_WhenValidDataProvided()
        {
            // Arrange
            var controller = new BlogController(_context, _mockEnv.Object);
            var model = new BlogPost
            {
                Id = Guid.NewGuid(),
                Title = "ToDelete",
                AuthorName = "Test Author",
                AuthorEmail = "test@example.com",  // Required
                WebsiteLink = "https://example.com", // Required
                CreatedAt = DateTime.UtcNow
            };

            var pdfMock = new Mock<IFormFile>();
            var content = new MemoryStream(new byte[10]);
            pdfMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Returns(Task.CompletedTask);
            pdfMock.Setup(f => f.FileName).Returns("test.pdf");

            // Act
            var result = await controller.Create(model, pdfMock.Object, null);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Blog", redirect.ControllerName);
        }

        [Fact]
        public async Task Edit_ReturnsNotFound_WhenIdDoesNotMatch()
        {
            // Arrange
            var controller = new BlogController(_context, _mockEnv.Object);
            var post = new BlogPost { Id = Guid.NewGuid(), Title = "Test" };

            // Act
            var result = await controller.Edit(Guid.NewGuid(), post, null, null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Delete_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            var controller = new BlogController(_context, _mockEnv.Object);

            // Act
            var result = controller.Delete(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeleteConfirmed_RemovesBlogPost_AndRedirects()
        {
            // Arrange: use a fresh in-memory context
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // ensures test isolation
                .Options;

            var context = new ApplicationDbContext(options);

            var postToDelete = new BlogPost
            {
                Id = Guid.NewGuid(),
                Title = "Test Blog",
                AuthorName = "Test Author",
                AuthorEmail = "test@example.com",
                WebsiteLink = "https://example.com",
                CreatedAt = DateTime.UtcNow,
                PdfFilePath = "/uploads/blogs/test.pdf"
            };

            context.BlogPosts.Add(postToDelete);
            context.SaveChanges();

            var mockEnv = new Mock<IWebHostEnvironment>();
            var controller = new BlogController(context, mockEnv.Object);

            // Act
            var result = controller.DeleteConfirmed(postToDelete.Id) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Empty(context.BlogPosts); // ✅ confirms deletion
        }


    }
}
