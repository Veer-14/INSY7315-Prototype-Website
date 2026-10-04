using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PKValves.Tests;

public class AccountIntegrationTests
{
    private static Dictionary<string, string> LoginForm()
    {
        return new Dictionary<string, string>
        {
            ["Email"] = "customer@example.com",
            ["Password"] = "TestPassword123!",
            ["RememberMe"] = "false"
        };
    }

    private static Dictionary<string, string> RegisterForm()
    {
        return new Dictionary<string, string>
        {
            ["FullName"] = "Test Customer",
            ["Email"] = "customer@example.com",
            ["Phone"] = "0821234567",
            ["Password"] = "TestPassword123!",
            ["ConfirmPassword"] = "TestPassword123!"
        };
    }

    private static RecordedApiRequest AssertApiCall(
        AccountProductTestFactory factory,
        string path)
    {
        var request = Assert.Single(factory.Requests);

        Assert.Equal("POST", request.Method);
        Assert.Equal(path, request.Path);

        return request;
    }

    private static async Task AssertLoggedOutAsync(
        HttpClient client)
    {
        using var page = await client.GetAsync("/Contact");

        Assert.Equal(
            HttpStatusCode.OK,
            page.StatusCode);

        string html =
            await page.Content.ReadAsStringAsync();

        Assert.Contains(
            "Log in to send an enquiry",
            html);
    }

    [Fact]
    public async Task
        ValidLogin_SendsCredentials_AndSignsUserIn()
    {
        using var factory =
            new AccountProductTestFactory();

        factory.ApiReply = _ =>
            AccountProductTestFactory.JsonReply(
                HttpStatusCode.OK,
                new
                {
                    success = true,
                    uid = "test-customer",
                    fullName = "Test Customer",
                    email = "customer@example.com",
                    idToken = "simulated-token"
                });

        using var client = factory.CreateBrowser();

        using var response =
            await AccountProductTestForms.SubmitAsync(
                client,
                "/Account/Login",
                LoginForm());

        Assert.Equal(
            HttpStatusCode.Redirect,
            response.StatusCode);

        var request = AssertApiCall(
            factory,
            "/api/auth/login");

        using var payload =
            JsonDocument.Parse(request.Body);

        Assert.Equal(
            "customer@example.com",
            payload.RootElement
                .GetProperty("email")
                .GetString());

        Assert.Equal(
            "TestPassword123!",
            payload.RootElement
                .GetProperty("password")
                .GetString());

        // A separate request checks that the login cookie works.
        using var page =
            await client.GetAsync("/Contact");

        Assert.Equal(
            HttpStatusCode.OK,
            page.StatusCode);

        string html =
            await page.Content.ReadAsStringAsync();

        Assert.DoesNotContain(
            "Log in to send an enquiry",
            html);

        Assert.Contains("Send Enquiry", html);
        Assert.Contains("Test Customer", html);
    }

    [Fact]
    public async Task
        RejectedLogin_ShowsError_AndDoesNotSignUserIn()
    {
        using var factory =
            new AccountProductTestFactory();

        factory.ApiReply = _ =>
            AccountProductTestFactory.JsonReply(
                HttpStatusCode.Unauthorized,
                new
                {
                    success = false,
                    message = "Incorrect email or password."
                });

        using var client = factory.CreateBrowser();

        using var response =
            await AccountProductTestForms.SubmitAsync(
                client,
                "/Account/Login",
                LoginForm());

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.Contains(
            "Incorrect email or password.",
            await response.Content.ReadAsStringAsync());

        AssertApiCall(factory, "/api/auth/login");

        await AssertLoggedOutAsync(client);
    }

    [Theory]
    [InlineData("Email", "invalid-email")]
    [InlineData("Password", "")]
    public async Task InvalidLogin_DoesNotCallApi(
        string field,
        string value)
    {
        using var factory =
            new AccountProductTestFactory();

        using var client = factory.CreateBrowser();

        var fields = LoginForm();

        fields[field] = value;

        using var response =
            await AccountProductTestForms.SubmitAsync(
                client,
                "/Account/Login",
                fields);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.Empty(factory.Requests);

        await AssertLoggedOutAsync(client);
    }

    [Fact]
    public async Task
        LoginApiUnavailable_ShowsFriendlyError()
    {
        using var factory =
            new AccountProductTestFactory();

        factory.ApiReply = _ =>
            throw new HttpRequestException(
                "Simulated connection failure.");

        using var client = factory.CreateBrowser();

        using var response =
            await AccountProductTestForms.SubmitAsync(
                client,
                "/Account/Login",
                LoginForm());

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.Contains(
            "Unable to connect to the server. Please try again.",
            await response.Content.ReadAsStringAsync());

        AssertApiCall(factory, "/api/auth/login");

        await AssertLoggedOutAsync(client);
    }

    [Fact]
    public async Task
        ValidRegistration_SendsDetails_AndRedirectsToLogin()
    {
        using var factory =
            new AccountProductTestFactory();

        factory.ApiReply = _ =>
            AccountProductTestFactory.JsonReply(
                HttpStatusCode.OK,
                new
                {
                    success = true,
                    uid = "test-customer",
                    email = "customer@example.com"
                });

        using var client = factory.CreateBrowser();

        using var response =
            await AccountProductTestForms.SubmitAsync(
                client,
                "/Account/Register",
                RegisterForm());

        Assert.Equal(
            HttpStatusCode.Redirect,
            response.StatusCode);

        Assert.Equal(
            "/Account/Login",
            response.Headers.Location?.OriginalString);

        var request = AssertApiCall(
            factory,
            "/api/auth/register");

        using var payload =
            JsonDocument.Parse(request.Body);

        Assert.Equal(
            "Test Customer",
            payload.RootElement
                .GetProperty("fullName")
                .GetString());

        Assert.Equal(
            "customer@example.com",
            payload.RootElement
                .GetProperty("email")
                .GetString());

        Assert.Equal(
            "0821234567",
            payload.RootElement
                .GetProperty("phone")
                .GetString());

        Assert.Equal(
            "TestPassword123!",
            payload.RootElement
                .GetProperty("password")
                .GetString());

        Assert.False(
            payload.RootElement.TryGetProperty(
                "confirmPassword",
                out _));

        using var page =
            await client.GetAsync("/Account/Login");

        Assert.Equal(
            HttpStatusCode.OK,
            page.StatusCode);

        Assert.Contains(
            "Account created successfully. Please log in.",
            await page.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("Email", "invalid-email")]
    [InlineData("Password", "123")]
    [InlineData(
        "ConfirmPassword",
        "DifferentPassword123!")]
    public async Task
        InvalidRegistration_ShowsValidation_AndDoesNotCallApi(
            string field,
            string value)
    {
        using var factory =
            new AccountProductTestFactory();

        using var client = factory.CreateBrowser();

        var fields = RegisterForm();

        fields[field] = value;

        // Keep both passwords matching when testing length.
        if (field == "Password")
        {
            fields["ConfirmPassword"] = value;
        }

        using var response =
            await AccountProductTestForms.SubmitAsync(
                client,
                "/Account/Register",
                fields);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.Empty(factory.Requests);

        string html =
            await response.Content.ReadAsStringAsync();

        var span = Regex.Match(
            html,
            $@"<span\b[^>]*data-valmsg-for=""{field}""[^>]*>.*?</span>",
            RegexOptions.Singleline);

        Assert.True(span.Success);

        Assert.Contains(
            "field-validation-error",
            span.Value);
    }

    [Fact]
    public async Task
        RejectedRegistration_ShowsApiError_AndKeepsEmail()
    {
        using var factory =
            new AccountProductTestFactory();

        factory.ApiReply = _ =>
            AccountProductTestFactory.JsonReply(
                HttpStatusCode.BadRequest,
                new
                {
                    success = false,
                    message =
                        "This email is already registered."
                });

        using var client = factory.CreateBrowser();

        using var response =
            await AccountProductTestForms.SubmitAsync(
                client,
                "/Account/Register",
                RegisterForm());

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        string html =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "This email is already registered.",
            html);

        Assert.Contains(
            "customer@example.com",
            html);

        AssertApiCall(
            factory,
            "/api/auth/register");
    }
}