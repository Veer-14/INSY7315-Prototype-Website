using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PKValves.Models;
using Xunit;

namespace PKValves.Tests
{
    public class ContactValidationTests
    {
        private static ContactViewModel CreateValidEnquiry()
        {
            return new ContactViewModel
            {
                Name = "Test Customer",
                Email = "customer@example.com",
                Phone = null,
                Subject = "Product Enquiry",
                Message = "Please send information about your valves."
            };
        }

        private static List<ValidationResult> Validate(
            ContactViewModel model)
        {
            var errors = new List<ValidationResult>();

            Validator.TryValidateObject(
                model,
                new ValidationContext(model),
                errors,
                validateAllProperties: true);

            return errors;
        }

        [Fact]
        public void ValidEnquiry_WithoutOptionalPhone_IsAccepted()
        {
            var model = CreateValidEnquiry();

            var errors = Validate(model);

            Assert.Empty(errors);
        }

        [Theory]
        [InlineData("Name")]
        [InlineData("Email")]
        [InlineData("Subject")]
        [InlineData("Message")]
        public void MissingRequiredField_IsRejected(string field)
        {
            var model = CreateValidEnquiry();

            switch (field)
            {
                case "Name":
                    model.Name = "";
                    break;
                case "Email":
                    model.Email = "";
                    break;
                case "Subject":
                    model.Subject = "";
                    break;
                case "Message":
                    model.Message = "";
                    break;
            }

            var errors = Validate(model);

            Assert.Contains(
                errors,
                error => System.Linq.Enumerable.Contains(
                    error.MemberNames, field));
        }

        [Fact]
        public void InvalidEmail_IsRejected()
        {
            var model = CreateValidEnquiry();
            model.Email = "not-an-email-address";

            var errors = Validate(model);

            Assert.Contains(
                errors,
                error => System.Linq.Enumerable.Contains(
                    error.MemberNames, nameof(ContactViewModel.Email)));
        }

        [Fact]
        public void SubjectOutsideAllowedOptions_IsRejected()
        {
            var model = CreateValidEnquiry();
            model.Subject = "Unexpected subject";

            var errors = Validate(model);

            Assert.Contains(
                errors,
                error => System.Linq.Enumerable.Contains(
                    error.MemberNames, nameof(ContactViewModel.Subject)));
        }

        [Theory]
        [InlineData(5000, true)]
        [InlineData(5001, false)]
        public void MessageLengthLimit_IsEnforced(
            int length,
            bool shouldBeValid)
        {
            var model = CreateValidEnquiry();
            model.Message = new string('A', length);

            var errors = Validate(model);

            if (shouldBeValid)
            {
                Assert.Empty(errors);
            }
            else
            {
                Assert.Contains(
                    errors,
                    error => System.Linq.Enumerable.Contains(
                        error.MemberNames,
                        nameof(ContactViewModel.Message)));
            }
        }
    }
}