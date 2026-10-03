using Microsoft.AspNetCore.Authentication.Cookies;
using PKValves.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout =
        TimeSpan.FromMinutes(30);

    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy =
        CookieSecurePolicy.Always;
});

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IContactEmailService, ContactEmailService>();

builder.Services.AddHttpClient<AccountApiService>(
    client =>
    {
        string baseUrl =
            builder.Configuration[
                "ApiSettings:BaseUrl"]
            ?? throw new InvalidOperationException(
                "API BaseUrl is missing.");

        client.BaseAddress =
            new Uri(baseUrl);
    });

builder.Services.AddHttpClient<ProductApiService>(
    client =>
    {
        string baseUrl =
            builder.Configuration[
                "ApiSettings:BaseUrl"]
            ?? throw new InvalidOperationException(
                "API BaseUrl is missing.");

        client.BaseAddress =
            new Uri(baseUrl);
    });

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath =
            "/Account/Login";

        options.AccessDeniedPath =
            "/Account/Login";

        options.ExpireTimeSpan =
            TimeSpan.FromHours(8);

        options.SlidingExpiration = true;

        options.Cookie.HttpOnly = true;

        options.Cookie.SecurePolicy =
            CookieSecurePolicy.Always;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSession();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}");

app.Run();
public partial class Program
{
}