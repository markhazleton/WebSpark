using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PromptSpark.Domain.Data;

public class GPTDbContextFactory : IDesignTimeDbContextFactory<GPTDbContext>
{
    public GPTDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GPTDbContext>();
        optionsBuilder.UseSqlite("Data Source=c:\\websites\\WebSpark\\PromptSpark.db");

        return new GPTDbContext(optionsBuilder.Options);
    }
}
