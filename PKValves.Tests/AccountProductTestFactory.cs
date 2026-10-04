using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PKValves.Services;

namespace PKValves.Tests;

// Stores information about requests sent to the simulated API.
public record RecordedApiRequest(
    string Method,
    string Path,
    string Body);

public class AccountProductTestFactory
    : WebApplicationFactory<Program>
{
    public List<RecordedApiRequest> Requests { get; } = new();

    // Each test chooses what the simulated API returns.
    public Func<HttpRequestMessage, HttpResponseMessage> ApiReply
    { get; set; }
        = _ => throw new InvalidOperationException(
            "Unexpected API request.");

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(
            (context, configuration) =>
            {
                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ApiSettings:BaseUrl"] =
                            "https://test-api.invalid/"
                    });
            });

        builder.ConfigureTestServices(services =>
        {
            // Reuse the fake sender from the existing enquiry tests.
            services.RemoveAll<IContactEmailService>();

            services.AddSingleton<IContactEmailService>(
                new FakeContactEmailService());

            // Replace outgoing API connections during these tests.
            services.AddHttpClient<AccountApiService>()
                .ConfigurePrimaryHttpMessageHandler(
                    () => new SimulatedApiHandler(this));

            services.AddHttpClient<ProductApiService>()
                .ConfigurePrimaryHttpMessageHandler(
                    () => new SimulatedApiHandler(this));

            services.AddHttpClient<WishlistApiService>()
                .ConfigurePrimaryHttpMessageHandler(
                    () => new SimulatedApiHandler(this));
        });
    }

    public HttpClient CreateBrowser()
    {
        return CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),

                // Allow tests to inspect redirect responses.
                AllowAutoRedirect = false,

                // Preserve authentication and security cookies.
                HandleCookies = true
            });
    }

    public static HttpResponseMessage JsonReply(
        HttpStatusCode status,
        object body)
    {
        return new HttpResponseMessage(status)
        {
            Content = JsonContent.Create(body)
        };
    }

    private sealed class SimulatedApiHandler
        : HttpMessageHandler
    {
        private readonly AccountProductTestFactory _factory;

        public SimulatedApiHandler(
            AccountProductTestFactory factory)
        {
            _factory = factory;
        }

        protected override async Task<HttpResponseMessage>
            SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
        {
            string body = request.Content is null
                ? ""
                : await request.Content.ReadAsStringAsync(
                    cancellationToken);

            _factory.Requests.Add(
                new RecordedApiRequest(
                    request.Method.Method,
                    request.RequestUri!.AbsolutePath,
                    body));

            return _factory.ApiReply(request);
        }
    }
}

public static class AccountProductTestForms
{
    public static async Task<HttpResponseMessage> SubmitAsync(
        HttpClient client,
        string page,
        Dictionary<string, string> fields)
    {
        // Open the form to obtain its security token and cookie.
        using var formPage = await client.GetAsync(page);

        formPage.EnsureSuccessStatusCode();

        string html =
            await formPage.Content.ReadAsStringAsync();

        var input = Regex.Match(
            html,
            @"<input\b[^>]*\bname=""__RequestVerificationToken""[^>]*>");

        Assert.True(
            input.Success,
            "The form should include an anti-forgery token.");

        var value = Regex.Match(
            input.Value,
            @"\bvalue=""([^""]*)""");

        Assert.True(
            value.Success,
            "The security token should have a value.");

        fields["__RequestVerificationToken"] =
            WebUtility.HtmlDecode(value.Groups[1].Value);

        using var content =
            new FormUrlEncodedContent(fields);

        return await client.PostAsync(page, content);
    }
}