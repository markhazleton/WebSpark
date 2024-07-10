using Microsoft.EntityFrameworkCore.Design;

namespace WebSpark.Core.Data;

public class WebSparkDbContextFactory : IDesignTimeDbContextFactory<WebSparkDbContext>
{
    public WebSparkDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WebSparkDbContext>();
        optionsBuilder.UseSqlite("Data Source=c:\\websites\\WebSpark\\WebSpark.db");

        return new WebSparkDbContext(optionsBuilder.Options);
    }
}
