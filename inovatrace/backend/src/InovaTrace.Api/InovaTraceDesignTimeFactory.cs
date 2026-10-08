using InovaTrace.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InovaTrace.Api;

public sealed class InovaTraceDesignTimeFactory : IDesignTimeDbContextFactory<InovaTraceDbContext>
{
    public InovaTraceDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<InovaTraceDbContext>()
            .UseNpgsql("Host=localhost;Port=5433;Database=inovatrace;Username=inovatrace;Password=inovatrace")
            .Options;
        return new InovaTraceDbContext(options);
    }
}
