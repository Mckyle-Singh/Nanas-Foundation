using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nanas_Foundation.Models;
using Xunit;
using FluentAssertions;


namespace Nanas_Foundation.Tests.Models
{
    public class DonationTests
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
        public void Donation_WithValidProperties_ShouldBeValid()
        {
            // Arrange
            var donation = new Donation
            {
                Amount = 100m,
                Bank = "Standard Bank",
                Notes = "Test donation",
                DonationDate = DateTime.UtcNow,
                StripeSessionId = "sess_12345"
            };

            // Act
            var results = ValidateModel(donation);

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public void Donation_WithoutRequiredFields_ShouldFailValidation()
        {
            // Arrange
            var donation = new Donation(); // Missing Amount, Bank, DonationDate defaults to now

            // Act
            var results = ValidateModel(donation);

            // Assert
            results.Should().ContainSingle(r => r.MemberNames.Contains("Amount"));
            results.Should().ContainSingle(r => r.MemberNames.Contains("Bank"));
            // DonationDate has default value, so it should be valid
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-50)]
        [InlineData(10000001)]
        public void Donation_WithInvalidAmount_ShouldFailValidation(decimal invalidAmount)
        {
            // Arrange
            var donation = new Donation
            {
                Amount = invalidAmount,
                Bank = "Standard Bank",
                DonationDate = DateTime.UtcNow
            };

            // Act
            var results = ValidateModel(donation);

            // Assert
            results.Should().ContainSingle(r => r.MemberNames.Contains("Amount"));
        }

        [Fact]
        public void Donation_BankExceedsMaxLength_ShouldFailValidation()
        {
            // Arrange
            var donation = new Donation
            {
                Amount = 100m,
                Bank = new string('B', 101), // 101 chars, max is 100
                DonationDate = DateTime.UtcNow
            };

            // Act
            var results = ValidateModel(donation);

            // Assert
            results.Should().ContainSingle(r => r.MemberNames.Contains("Bank"));
        }

        [Fact]
        public void Donation_NotesExceedsMaxLength_ShouldFailValidation()
        {
            // Arrange
            var donation = new Donation
            {
                Amount = 100m,
                Bank = "Standard Bank",
                Notes = new string('N', 501), // max 500
                DonationDate = DateTime.UtcNow
            };

            // Act
            var results = ValidateModel(donation);

            // Assert
            results.Should().ContainSingle(r => r.MemberNames.Contains("Notes"));
        }

        [Fact]
        public void Donation_StripeSessionIdExceedsMaxLength_ShouldFailValidation()
        {
            // Arrange
            var donation = new Donation
            {
                Amount = 100m,
                Bank = "Standard Bank",
                DonationDate = DateTime.UtcNow,
                StripeSessionId = new string('S', 201) // max 200
            };

            // Act
            var results = ValidateModel(donation);

            // Assert
            results.Should().ContainSingle(r => r.MemberNames.Contains("StripeSessionId"));
        }
    }
}
