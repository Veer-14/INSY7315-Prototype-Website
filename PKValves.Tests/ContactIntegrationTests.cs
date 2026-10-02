using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace PKValves.Tests
{
    public class ContactIntegrationTests
    {
        private static async Task<Dictionary<string, string>>
            CreateFormAsync(HttpClient client)
        {
            // Load the real page to obtain its security token.
            using var response = await client.GetAsync("/Contact");

            response.EnsureSuccessStatusCode();

            string html = await response.Content.ReadAsStringAsync();

            var tokenInput = Regex.Match(
                html,
                @"<input\b[^>]*\bname=""__RequestVerificationToken""[^>]*>",
                RegexOptions.IgnoreCase);

            Assert.True(
                tokenInput.Success,
                "The contact form should contain an antiforgery token.");

            var tokenValue = Regex.Match(
                tokenInput.Value,
                @"\bvalue=""([^""]+)""",
                RegexOptions.IgnoreCase);

            Assert.True(
                tokenValue.Success,
                "The antiforgery token should have a value.");

            return new Dictionary<string, string>
            {
                ["Name"] = "Test Customer",
                ["Email"] = "customer@example.com",
                ["Phone"] = "",
                ["Subject"] = "Product Enquiry",
                ["Message"] = "Please send valve specifications.",
                ["__RequestVerificationToken"] =
                    WebUtility.HtmlDecode(
                        tokenValue.Groups[1].Value)
            };
        }

        [Fact]
        public async Task LoggedOutUser_IsRedirectedToLogin()
        {
            using var factory = new ContactWebApplicationFactory();
            using var client = factory.CreateTestClient(loggedIn: false);

            var form = await CreateFormAsync(client);

            using var response = await client.PostAsync(
                "/Contact/Submit",
                new FormUrlEncodedContent(form));

            Assert.Equal(
                HttpStatusCode.Redirect, response.StatusCode);

            Assert.Contains(
                "/Account/Login",
                response.Headers.Location?.ToString() ?? "");

            Assert.Empty(factory.EmailSender.SentEnquiries);
        }

        [Fact]
        public async Task InvalidEnquiry_ShowsError_WithoutSending()
        {
            using var factory = new ContactWebApplicationFactory();
            using var client = factory.CreateTestClient(loggedIn: true);

            var form = await CreateFormAsync(client);
            form["Message"] = "";

            using var response = await client.PostAsync(
                "/Contact/Submit",
                new FormUrlEncodedContent(form));

            string html = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.Contains(
                "Please enter your message.", html);

            Assert.Empty(factory.EmailSender.SentEnquiries);
        }

        [Fact]
        public async Task ValidEnquiry_IsSent_AndShowsSuccess()
        {
            using var factory = new ContactWebApplicationFactory();
            using var client = factory.CreateTestClient(loggedIn: true);

            var form = await CreateFormAsync(client);

            using var response = await client.PostAsync(
                "/Contact/Submit",
                new FormUrlEncodedContent(form));

            Assert.Equal(
                HttpStatusCode.Redirect, response.StatusCode);

            var enquiry = Assert.Single(
                factory.EmailSender.SentEnquiries);

            Assert.Equal("Test Customer", enquiry.Name);
            Assert.Equal("customer@example.com", enquiry.Email);
            Assert.Equal("Product Enquiry", enquiry.Subject);
            Assert.Equal(
                "Please send valve specifications.",
                enquiry.Message);

            Assert.NotNull(response.Headers.Location);

            // Follow the redirect and check the displayed feedback.
            using var page = await client.GetAsync(
                response.Headers.Location!);

            page.EnsureSuccessStatusCode();

            string html = await page.Content.ReadAsStringAsync();

            Assert.Contains(
                "Your enquiry has been submitted successfully.",
                html);
        }

        [Fact]
        public async Task EmailFailure_ShowsError_AndKeepsMessage()
        {
            using var factory = new ContactWebApplicationFactory();
            factory.EmailSender.ShouldFail = true;

            using var client = factory.CreateTestClient(loggedIn: true);

            var form = await CreateFormAsync(client);

            using var response = await client.PostAsync(
                "/Contact/Submit",
                new FormUrlEncodedContent(form));

            string html = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.Contains(
                "We could not confirm that your enquiry was sent.",
                html);

            Assert.Contains(
                "Please send valve specifications.", html);

            Assert.DoesNotContain(
                "Your enquiry has been submitted successfully.",
                html);

            Assert.Empty(factory.EmailSender.SentEnquiries);
        }

        [Fact]
        public async Task MissingSecurityToken_IsRejected()
        {
            using var factory = new ContactWebApplicationFactory();
            using var client = factory.CreateTestClient(loggedIn: true);

            var form = await CreateFormAsync(client);
            form.Remove("__RequestVerificationToken");

            using var response = await client.PostAsync(
                "/Contact/Submit",
                new FormUrlEncodedContent(form));

            Assert.Equal(
                HttpStatusCode.BadRequest, response.StatusCode);

            Assert.Empty(factory.EmailSender.SentEnquiries);
        }
    }
}