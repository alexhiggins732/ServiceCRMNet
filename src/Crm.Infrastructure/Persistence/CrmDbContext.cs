using Crm.Domain.Common;
using Crm.Domain.Entities;
using Crm.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence;

public class CrmDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public CrmDbContext(DbContextOptions<CrmDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<Automation> Automations { get; set; } = null!;
    public DbSet<IdempotencyKey> IdempotencyKeys { get; set; } = null!;
    public DbSet<FailedJob> FailedJobs { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Lead> Leads { get; set; } = null!;
    public DbSet<Job> Jobs { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<ProductService> ProductsServices { get; set; } = null!;
    public DbSet<PricebookTemplate> PricebookTemplates { get; set; } = null!;
    public DbSet<TemplateItem> TemplateItems { get; set; } = null!;
    public DbSet<Estimate> Estimates { get; set; } = null!;
    public DbSet<EstimateItem> EstimateItems { get; set; } = null!;
    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<InvoiceItem> InvoiceItems { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<AiConversation> AiConversations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply global query filter for multi-tenancy
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(CrmDbContext).GetMethod(nameof(SetGlobalQueryFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var genericMethod = method?.MakeGenericMethod(entityType.ClrType);
                genericMethod?.Invoke(this, new object[] { builder });
            }
        }

        // Constraints and indexes
        builder.Entity<User>().HasIndex(u => new { u.TenantId, u.Email }).IsUnique();
        builder.Entity<Tenant>().HasIndex(t => t.Domain).IsUnique();
    }

    private void SetGlobalQueryFilter<T>(ModelBuilder builder) where T : TenantEntity
    {
        builder.Entity<T>().HasQueryFilter(e => EF.Property<Guid>(e, "TenantId") == _tenantService.GetCurrentTenantId());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        foreach (var entry in ChangeTracker.Entries<TenantEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (tenantId.HasValue)
                {
                    entry.Entity.TenantId = tenantId.Value;
                }
                else
                {
                    throw new InvalidOperationException("Cannot save tenant entity without a tenant context.");
                }
            }
        }

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
