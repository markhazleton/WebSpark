using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebSpark.UserIdentity.Data;
public class WebSparkUserContextFactory : IDesignTimeDbContextFactory<WebSparkUserContext>
{
    public WebSparkUserContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WebSparkUserContext>();
        optionsBuilder.UseSqlite("Data Source=c:\\websites\\WebSpark\\ControlSparkUser.db");

        return new WebSparkUserContext(optionsBuilder.Options);
    }
}
