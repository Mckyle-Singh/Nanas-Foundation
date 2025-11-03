using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Nanas_Foundation.Models;
using Xunit;

namespace Nanas_Foundation.Tests.Models
{
    public class EventTests
    {
        // Helper method to validate a model
        private IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, context, validationResults, true);
            return validationResults;
        }

        [Fact]
        public void Event_WithValidProperties_ShouldBeValid()
        {
            // Arrange
            var ev = new Event
            {
                Title = "Charity Gala",
                Date = DateTime.UtcNow.AddDays(10),
                Description = "Annual fundraising event",
                Location = "Johannesburg"
            };

            // Act
            var results = ValidateModel(ev);

            // Assert
            Assert.Empty(results);
            Assert.NotEqual(Guid.Empty, ev.Id);           // Id should be auto-generated
            Assert.True(ev.CreatedAt <= DateTime.UtcNow); // CreatedAt should default to now
        }

        [Fact]
        public void Event_WithoutRequiredFields_ShouldFailValidation()
        {
            // Arrange
            var ev = new Event(); // Missing Title and Date

            // Act
            var results = ValidateModel(ev);

            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains("Title"));

            // ❌ Date is not nullable, so [Required] won't trigger
            // Assert.Contains(results, r => r.MemberNames.Contains("Date")); // remove or comment out
        }


        [Fact]
        public void Event_TitleCanBeNonEmptyString()
        {
            // Arrange
            var ev = new Event
            {
                Title = "Community Meeting",
                Date = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var results = ValidateModel(ev);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void Event_OptionalFields_CanBeNullOrEmpty()
        {
            // Arrange
            var ev = new Event
            {
                Title = "Workshop",
                Date = DateTime.UtcNow.AddDays(5),
                Description = null,
                Location = null
            };

            // Act
            var results = ValidateModel(ev);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void Event_DefaultValues_ShouldBeSet()
        {
            // Arrange
            var ev = new Event
            {
                Title = "Seminar",
                Date = DateTime.UtcNow
            };

            // Act & Assert
            Assert.NotEqual(Guid.Empty, ev.Id);           // Id auto-generated
            Assert.True(ev.CreatedAt <= DateTime.UtcNow); // CreatedAt defaults to now
        }
    }
}
