using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace PromptSpark.Areas.Identity.Data;

public class AdminContext : IdentityDbContext<AdminUser>
{
    public AdminContext(DbContextOptions<AdminContext> options)
        : base(options)
    {
    }
}
