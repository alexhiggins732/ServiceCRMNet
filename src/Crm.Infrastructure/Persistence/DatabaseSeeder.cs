using Crm.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Crm.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly CrmDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(CrmDbContext context, UserManager<User> userManager, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            if (!_context.Tenants.Any())
            {
                _logger.LogInformation("Seeding initial data...");

                var tenant = new Tenant
                {
                    Id = Guid.NewGuid(),
                    Name = "Demo Garage Doors",
                    Domain = "demo"
                };

                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                var adminEmail = "admin@example.com";
                if (await _userManager.FindByEmailAsync(adminEmail).ConfigureAwait(false) == null)
                {
                    var admin = new User
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FirstName = "Admin",
                        LastName = "User",
                        TenantId = tenant.Id,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(admin, "YourStrong@Passw0rd!").ConfigureAwait(false);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Created admin user: {Email}", adminEmail);
                    }
                    else
                    {
                        _logger.LogError("Failed to create admin user");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
