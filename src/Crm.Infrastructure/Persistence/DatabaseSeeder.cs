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

                var tenantId = Guid.NewGuid();
                var tenant = new Tenant
                {
                    Id = tenantId,
                    Name = "Demo Garage Doors",
                    Domain = "demo"
                };

                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();

                var adminEmail = "admin@example.com";
                if (await _userManager.FindByEmailAsync(adminEmail) == null)
                {
                    var admin = new User
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FirstName = "Admin",
                        LastName = "User",
                        TenantId = tenantId,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(admin, "YourStrong@Passw0rd!");
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Created admin user: {Email}", adminEmail);
                    }
                    else
                    {
                        _logger.LogError("Failed to create admin user");
                    }
                }

                // Seed Demo Data
                var customerId = Guid.NewGuid();
                _context.Customers.Add(new Customer { Id = customerId, TenantId = tenantId, FirstName = "Alice", LastName = "Johnson", Email = "alice@example.com", Phone = "555-0101", Address = "123 Maple St" });
                _context.Customers.Add(new Customer { TenantId = tenantId, FirstName = "Bob", LastName = "Smith", Email = "bob@example.com", Phone = "555-0202", Address = "456 Oak Ave" });

                var leadId = Guid.NewGuid();
                _context.Leads.Add(new Lead { Id = leadId, TenantId = tenantId, Name = "Fix spring", Status = "New", Source = "Website" });

                var jobId = Guid.NewGuid();
                _context.Jobs.Add(new Job { Id = jobId, TenantId = tenantId, Title = "Spring repair", Description = "Needs new tension spring", Status = "Scheduled", CustomerId = customerId });

                _context.Messages.Add(new Message { TenantId = tenantId, CustomerId = customerId, Channel = Crm.Domain.Enums.MessageChannel.Sms, Direction = Crm.Domain.Enums.MessageDirection.Inbound, Body = "Hi, my garage door won't open.", ExternalId = "ext-1", ReceivedAt = DateTime.UtcNow.AddHours(-2) });

                var prodId = Guid.NewGuid();
                var prod = new ProductService { Id = prodId, TenantId = tenantId, Sku = "SPRING-01", Name = "Torsion Spring Replacement", UnitPrice = 150.00m, UnitType = "Each", Taxable = true };
                _context.ProductsServices.Add(prod);

                var templateId = Guid.NewGuid();
                _context.PricebookTemplates.Add(new PricebookTemplate { Id = templateId, TenantId = tenantId, Name = "Standard Spring Repair", Description = "Replace 2 torsion springs" });

                _context.TemplateItems.Add(new TemplateItem { TenantId = tenantId, PricebookTemplateId = templateId, ProductServiceId = prodId, Quantity = 2 });

                await _context.SaveChangesAsync();
                _logger.LogInformation("Demo data seeded successfully.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
