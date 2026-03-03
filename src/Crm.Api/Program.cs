using System.Text;
using Crm.Modules.AiAssistant;
using Crm.Infrastructure;
using Crm.Infrastructure.Middleware;
using Crm.Infrastructure.Tenancy;
using Crm.Modules.Integrations.Meta;
using Crm.Modules.Integrations.Twilio;
using Crm.Modules.Pricing;
using Crm.Modules.CoreCrm;
using Crm.Infrastructure.Persistence;
using Crm.Shared.Modules;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddIdentityCore<Crm.Domain.Entities.User>().AddEntityFrameworkStores<Crm.Infrastructure.Persistence.CrmDbContext>();

// Auth
var jwtSecret = builder.Configuration["JWT_SECRET"] ?? "ThisIsASecretKeyForJwtAuthenticationThatNeedsToBeLongEnoughToWork!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT_ISSUER"] ?? "CrmApp",
            ValidAudience = builder.Configuration["JWT_AUDIENCE"] ?? "CrmUsers",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });
builder.Services.AddAuthorization();

// Modules Discovery
var modules = new List<ICrmModule>
{
    new MetaModule(),
    new TwilioModule(),
    new AiAssistantModule(),
    new PricingModule(),
    new CoreCrmModule()
};

foreach (var module in modules)
{
    module.RegisterServices(builder.Services, builder.Configuration);
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var maxRetries = 5;
    for (int i = 1; i <= maxRetries; i++)
    {
        try
        {
            var seeder = scope.ServiceProvider.GetRequiredService<Crm.Infrastructure.Persistence.DatabaseSeeder>();
            await seeder.SeedAsync().ConfigureAwait(false);
            logger.LogInformation("Database seeded successfully.");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to seed database on attempt {Attempt}. Retrying...", i);
            if (i == maxRetries)
            {
                logger.LogCritical("Could not seed the database after multiple attempts. The database might not exist or migrations haven't been applied yet. Run 'dev.ps1 migrate'.");
                // Do NOT throw here, let the app start so health checks pass, but note that CRUD will fail.
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Infrastructure Middlewares
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<IdempotencyMiddleware>(); // Keep before authentication/routing if needed, or after
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantResolutionMiddleware>();

// Hangfire
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    // Authorization = new[] { new HangfireAuthorizationFilter() } // Add proper auth for prod
});

// Map Endpoints
app.MapGet("/health", () => "Healthy").WithTags("System");
app.MapPost("/auth/login", () =>
{
    // Return dummy JWT token for stub
    return Results.Ok(new { Token = "dummy-jwt-token" });
}).WithTags("Auth");
app.MapGet("/me", () => Results.Ok(new { User = "admin", TenantId = "default" })).RequireAuthorization().WithTags("Auth");

foreach (var module in modules)
{
    module.MapEndpoints(app);
}

await app.RunAsync();
