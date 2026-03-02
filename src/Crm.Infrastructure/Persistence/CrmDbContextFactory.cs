using Crm.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Crm.Infrastructure.Persistence;

public class CrmDbContextFactory : IDesignTimeDbContextFactory<CrmDbContext>
{
    public CrmDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CrmDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=CrmDb;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new CrmDbContext(optionsBuilder.Options, new DummyTenantService());
    }
}

public class DummyTenantService : ITenantService
{
    public Guid? GetCurrentTenantId() => null;
    public void SetCurrentTenantId(Guid tenantId) { }
}
