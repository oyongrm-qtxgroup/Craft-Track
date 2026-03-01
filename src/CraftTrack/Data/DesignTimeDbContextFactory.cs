using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CraftTrack.Data;

/// <summary>
/// Factory for creating DbContext instances at design time (for EF Core migrations).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CraftTrackDbContext>
{
    public CraftTrackDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CraftTrackDbContext>();
        optionsBuilder.UseSqlite($"Data Source={CraftTrackDbContext.GetDatabasePath()}");
        return new CraftTrackDbContext(optionsBuilder.Options);
    }
}
