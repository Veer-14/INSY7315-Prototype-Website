using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PKValves.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<FirebaseAuthService>();

builder.Services.AddSingleton<FirestoreService>();


// =====================================================
// FIREBASE PROJECT ID
// =====================================================

string firebaseProjectId =
    Environment.GetEnvironmentVariable("Firebase__ProjectId")
    ?? builder.Configuration["Firebase:ProjectId"]
    ?? throw new InvalidOperationException(
        "Firebase ProjectId is missing.");


// =====================================================
// FIREBASE JWT AUTHENTICATION
// =====================================================

builder.Services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority =
            $"https://securetoken.google.com/{firebaseProjectId}";

        options.Audience =
            firebaseProjectId;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidIssuer =
                    $"https://securetoken.google.com/{firebaseProjectId}",

                ValidateAudience = true,

                ValidAudience =
                    firebaseProjectId,

                ValidateLifetime = true
            };
    });

builder.Services.AddAuthorization();


// =====================================================
// BUILD APPLICATION
// =====================================================

var app = builder.Build();


// =====================================================
// SWAGGER
// =====================================================

// Enabled on Azure as well as locally
app.UseSwagger();
app.UseSwaggerUI();


// =====================================================
// MIDDLEWARE
// =====================================================

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

