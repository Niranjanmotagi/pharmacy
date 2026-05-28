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

// JWT secret priority:
//   1) JWT__KEY env var (Render / production)
//   2) Jwt:Key from appsettings.json (local dev)
//   3) Hardcoded dev fallback (last resort)
var jwtKey =
    Environment.GetEnvironmentVariable("JWT__KEY")
    ?? builder.Configuration["Jwt:Key"]
    ?? "THIS_IS_MY_SUPER_SECRET_JWT_KEY_123456789"; // dev fallback only

// Connection string priority:
//   1) DEFAULT_CONNECTION env var (Render / production)
//   2) ConnectionStrings:DefaultConnection from appsettings.json (local dev)
//
// Env var has to win, otherwise the localhost dev value in appsettings.json
// would always be used and Render would never connect to Neon.
var connectionString =
    Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No Postgres connection string found. Set the DEFAULT_CONNECTION env var " +
        "(production) or ConnectionStrings:DefaultConnection in appsettings.json (dev).");
}

// Comma-separated list of allowed front-end origins.
// Env var wins so Render can override the empty appsettings default.
//   CORS__ALLOWEDORIGINS = "https://your-app.vercel.app,https://other.example"
var allowedOrigins =
    (Environment.GetEnvironmentVariable("CORS__ALLOWEDORIGINS")
        ?? builder.Configuration["Cors:AllowedOrigins"]
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

// Schema bootstrap for hackathon deploys: build the tables directly from the
// model and apply HasData seeds. EnsureCreated is idempotent — it returns
// false if any table already exists, so it's safe to call on every cold start.
//
// If you need real migrations later, replace this block with:
//     db.Database.Migrate();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("==> Schema bootstrap: calling EnsureCreated()");
        var created = db.Database.EnsureCreated();
        logger.LogInformation(
            "==> EnsureCreated() returned {Created}. (true = schema + seeds just built; false = already existed)",
            created);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "==> Schema bootstrap FAILED.");
    }
}

app.Run();

/// <summary>
/// Resolved JWT options, injected into controllers that need to sign tokens.
/// </summary>
public record JwtOptions(string Key);
