using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using HackathonBackend.Data;
using HackathonBackend.Services;
using HackathonBackend.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =========================
// CONFIGURATION (env-driven for deploy)
// =========================

// JWT secret comes from configuration or env var:
//   JWT__KEY   (Render / Vercel style)
//   Jwt:Key    (appsettings)
var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? Environment.GetEnvironmentVariable("JWT__KEY")
    ?? "THIS_IS_MY_SUPER_SECRET_JWT_KEY_123456789"; // dev fallback only

// Connection string priority:
//   1) ConnectionStrings:DefaultConnection
//   2) DEFAULT_CONNECTION env var (Render injects env vars; this is the
//      simplest way to wire Azure SQL from there)
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No SQL Server connection string found. Set ConnectionStrings:DefaultConnection " +
        "or the DEFAULT_CONNECTION env var.");
}

// Comma-separated list of allowed front-end origins:
//   CORS__ALLOWEDORIGINS = "https://your-app.vercel.app,https://other.example"
// Falls back to wide-open for local dev.
var allowedOrigins =
    (builder.Configuration["Cors:AllowedOrigins"]
        ?? Environment.GetEnvironmentVariable("CORS__ALLOWEDORIGINS")
        ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

// =========================
// SERVICES
// =========================

builder.Services.AddControllers();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kv => kv.Value?.Errors.Count > 0)
            .ToDictionary(
                kv => kv.Key,
                kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var firstError = errors.Values
            .SelectMany(v => v)
            .FirstOrDefault() ?? "Validation failed.";

        return new BadRequestObjectResult(new
        {
            status = 400,
            title = "Validation failed",
            detail = firstError,
            message = firstError,
            errors
        });
    };
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
);

builder.Services.AddScoped<IEmailService, EmailService>();

// =========================
// JWT AUTHENTICATION
// =========================

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Make the resolved JWT key + connection string available to controllers if needed.
builder.Services.AddSingleton(new JwtOptions(jwtKey));

// =========================
// CORS
// =========================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // Local development fallback
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

// =========================
// OpenAPI (NET 10 native)
// =========================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// =========================
// FOLDERS
// =========================

var prescriptionsPath = Path.Combine(
    app.Environment.ContentRootPath, "wwwroot", "prescriptions");
if (!Directory.Exists(prescriptionsPath))
    Directory.CreateDirectory(prescriptionsPath);

var emailsPath = Path.Combine(
    app.Environment.ContentRootPath, "wwwroot", "emails");
if (!Directory.Exists(emailsPath))
    Directory.CreateDirectory(emailsPath);

// =========================
// PIPELINE
// =========================

app.UseMiddleware<GlobalExceptionMiddleware>();

// Always expose OpenAPI doc + Scalar UI; production hosts (Render free) often
// don't have an environment-aware way to expose dev-only routes.
app.MapOpenApi();
app.MapScalarApiReference("/swagger", options =>
{
    options.WithTitle("ByteBrigade Pharmacy API");
    options.WithOpenApiRoutePattern("/openapi/v1.json");
    options.WithPreferredScheme("Bearer");
    options.WithHttpBearerAuthentication(bearer =>
    {
        bearer.Token = "paste-your-jwt-token-here";
    });
});

// Render terminates HTTPS at the edge — its container only sees plain HTTP,
// so HTTPS redirection inside the container would fight the load balancer
// and produce "Failed to determine the https port" warnings.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Apply pending EF migrations on startup (Render container starts cold each
// time; this guarantees the schema is up to date on the cloud SQL host).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database migration failed at startup.");
    }
}

app.Run();

/// <summary>
/// Resolved JWT options, injected into controllers that need to sign tokens.
/// </summary>
public record JwtOptions(string Key);
