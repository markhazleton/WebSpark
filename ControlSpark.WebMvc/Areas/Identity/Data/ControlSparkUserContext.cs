using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ControlSpark.WebMvc.Areas.Identity.Data;

public class ControlSparkUserContext : IdentityDbContext<ControlSparkUser>
{
    public ControlSparkUserContext(DbContextOptions<ControlSparkUserContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }

    public DbSet<ControlSpark.Domain.EditModels.WebsiteEditModel> WebsiteEditModel { get; set; } = default!;
}
