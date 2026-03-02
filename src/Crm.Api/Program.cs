using Crm.AiAssistant;
using Crm.Infrastructure;
using Crm.Infrastructure.Middleware;
using Crm.Infrastructure.Tenancy;
using Crm.Integrations.Meta;
using Crm.Integrations.Twilio;
using Crm.Pricing;
using Crm.Shared.Modules;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

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
var modules = new List<IModule>
{
    new MetaModule(),
    new TwilioModule(),
    new AiAssistantModule(),
    new PricingModule()
};

foreach (var module in modules)
{
    module.RegisterServices(builder.Services, builder.Configuration);
}

var app = builder.Build();

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

// CRUD Stubs
var crmGroup = app.MapGroup("/api/crm").WithTags("CRM");
crmGroup.MapGet("/customers", () => Results.Ok(Array.Empty<object>()));
crmGroup.MapPost("/customers", () => Results.Ok());
crmGroup.MapGet("/leads", () => Results.Ok(Array.Empty<object>()));
crmGroup.MapPost("/leads", () => Results.Ok());
crmGroup.MapGet("/jobs", () => Results.Ok(Array.Empty<object>()));
crmGroup.MapPost("/jobs", () => Results.Ok());

// Availability Stub
app.MapGet("/api/availability", (string? date, string? serviceArea, int? durationMinutes) =>
{
    return Results.Ok(new[] {
        new { Time = "09:00", Available = true },
        new { Time = "10:00", Available = true }
    });
}).WithTags("Scheduling");

foreach (var module in modules)
{
    module.MapEndpoints(app);
}

app.Run();
