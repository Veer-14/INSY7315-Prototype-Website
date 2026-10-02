using System;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PKValves.Services;

namespace PKValves.Tests
{
    public class ContactWebApplicationFactory
        : WebApplicationFactory<Program>
    {
        public FakeContactEmailService EmailSender { get; } = new();

        protected override void ConfigureWebHost(
            IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                // Use the fake sender only inside the test server.
                services.RemoveAll<IContactEmailService>();

                services.AddSingleton<IContactEmailService>(
                    EmailSender);

                // Simulate authentication inside the test server.
                // Keep the website's normal cookie challenge,
                // which redirects logged-out users to the login page.
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        TestAuthenticationHandler.SchemeName;
                })
                .AddScheme<
                    AuthenticationSchemeOptions,
                    TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName,
                    options => { });
            });
        }

        public HttpClient CreateTestClient(bool loggedIn)
        {
            var client = CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false,
                    HandleCookies = true,
                    BaseAddress = new Uri("https://localhost")
                });

            if (loggedIn)
            {
                client.DefaultRequestHeaders.Add(
                    "X-Test-User", "customer");
            }

            return client;
        }
    }

    public class TestAuthenticationHandler
        : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "TestAuthentication";

        public TestAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult>
            HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("X-Test-User"))
            {
                return Task.FromResult(
                    AuthenticateResult.NoResult());
            }

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier, "test-customer"),
                new Claim(
                    ClaimTypes.Name, "Test Customer"),
                new Claim(
                    ClaimTypes.Email, "customer@example.com")
            };

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(
                principal, SchemeName);

            return Task.FromResult(
                AuthenticateResult.Success(ticket));
        }
    }
}