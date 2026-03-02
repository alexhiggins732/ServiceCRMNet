using Crm.Application.Common.Interfaces;
using Crm.Infrastructure.Middleware;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Tenancy;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

        if (string.IsNullOrEmpty(connectionString))
        {
            // For build-time tooling (EF Core tools)
            connectionString = "Server=(localdb)\\mssqllocaldb;Database=CrmDb;Trusted_Connection=True;MultipleActiveResultSets=true";
        }

        services.AddDbContext<CrmDbContext>(options =>
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(CrmDbContext).Assembly.FullName)));

        services.AddScoped<ITenantService, TenantService>();

        // Add Hangfire
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = Environment.ProcessorCount * 5;
        });

        // Add IDbContext implementation if you created one in Application layer
        // services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<CrmDbContext>());

        return services;
    }
}
