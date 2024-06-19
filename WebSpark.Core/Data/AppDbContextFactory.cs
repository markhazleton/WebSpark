using Microsoft.EntityFrameworkCore.Design;

namespace WebSpark.Core.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite("Data Source=c:\\websites\\WebSpark\\WebSpark.db");

        return new AppDbContext(optionsBuilder.Options);
    }
}
