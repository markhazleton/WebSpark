using Microsoft.EntityFrameworkCore.Diagnostics;

namespace WebSpark.Core.Data;


/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
public partial class WebSparkDbContext(DbContextOptions<WebSparkDbContext> options) : DbContext(options)
{
    protected readonly DbContextOptions<WebSparkDbContext> _options = options;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modelBuilder"></param>
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    private void UpdateDateTrackingFields()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity && (
                    e.State == EntityState.Added
                    || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            ((BaseEntity)entityEntry.Entity).UpdatedDate = DateTime.UtcNow;

            if (entityEntry.State == EntityState.Added)
            {
                ((BaseEntity)entityEntry.Entity).CreatedDate = DateTime.UtcNow;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WebSite>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("Id");

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(250);
            entity.HasIndex(e => e.Title).IsUnique();

            entity.Property(e => e.DomainUrl)
                .IsRequired()
                .HasMaxLength(250);
            entity.HasIndex(e => e.DomainUrl).IsUnique();

            entity.Property(e => e.GalleryFolder)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(e => e.Style)
                .IsRequired()
                .HasMaxLength(100);

        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.Property(e => e.Action)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Controller)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.KeyWords)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Icon).HasMaxLength(50);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Url).HasMaxLength(100);

            entity.HasOne(d => d.Domain)
                .WithMany(p => p.Menus)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Menu_Domain");

            entity.HasOne(d => d.Parent)
                .WithMany(p => p.InverseParent)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Menu_ParentMenu_ParentId");

        });
        // Many-to-many relationship between Menu and Keyword
        modelBuilder.Entity<Menu>()
            .HasMany(m => m.Keywords)
            .WithMany(k => k.Menus)
            .UsingEntity<Dictionary<string, object>>(
                "MenuKeyword",
                j => j.HasOne<Keyword>().WithMany().HasForeignKey("KeywordId"),
                j => j.HasOne<Menu>().WithMany().HasForeignKey("MenuId"));


        // Many-to-many relationship between ContentPart and Keyword
        modelBuilder.Entity<ContentPart>()
                .HasMany(cp => cp.Keywords)
                .WithMany(k => k.ContentParts)
                .UsingEntity<Dictionary<string, object>>(
                    "ContentPartKeyword",
                    j => j.HasOne<Keyword>().WithMany().HasForeignKey("KeywordId"),
                    j => j.HasOne<ContentPart>().WithMany().HasForeignKey("ContentPartId"));

        modelBuilder.Entity<Recipe>(entity =>
       {
           entity.Property(e => e.AuthorName)
               .IsRequired()
               .HasMaxLength(50);
           entity.Property(e => e.Ingredients)
               .IsRequired();
           entity.Property(e => e.Instructions)
               .IsRequired();
           entity.Property(e => e.Description)
               .HasMaxLength(500);
           entity.Property(e => e.Keywords)
               .HasMaxLength(100);
           entity.Property(e => e.Name)
               .IsRequired()
               .HasMaxLength(150);
           entity.HasOne(d => d.RecipeCategory)
                  .WithMany(p => p.Recipe)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Recipe_RecipeCategory")
                  .IsRequired();
           entity.HasOne(d => d.Domain);
       });

        modelBuilder.Entity<RecipeCategory>(entity =>
        {
            entity.Property(e => e.Comment)
                .HasMaxLength(1500);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(70);
            entity.HasOne(d => d.Domain);
            entity.HasMany(entity => entity.Recipe)
                .WithOne(entity => entity.RecipeCategory)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RecipeComment>(entity =>
        {
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(60);

            entity.Property(e => e.Comment).IsRequired();

            entity.HasOne(d => d.Recipe)
                .WithMany(p => p.RecipeComment)
                .OnDelete(DeleteBehavior.ClientCascade)
                .HasConstraintName("FK_RecipeComment_Recipe");
        });

        modelBuilder.Entity<RecipeImage>(entity =>
        {
            entity.Property(e => e.FileDescription).HasMaxLength(255);

            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(d => d.Recipe)
                .WithMany(p => p.RecipeImage)
                .OnDelete(DeleteBehavior.ClientCascade)
                .HasConstraintName("FK_RecipeImage_Recipe");
        });
        modelBuilder.Entity<PostCategory>()
    .HasKey(t => new { t.PostId, t.CategoryId });

        modelBuilder.Entity<PostCategory>()
            .HasOne(pt => pt.Post)
            .WithMany(p => p.PostCategories)
            .HasForeignKey(pt => pt.PostId);

        modelBuilder.Entity<PostCategory>()
            .HasOne(pt => pt.Category)
            .WithMany(t => t.PostCategories)
            .HasForeignKey(pt => pt.CategoryId);

        string sql = "getdate()";

        if (_options.Extensions != null)
        {
            foreach (var ext in _options.Extensions)
            {
                if (ext.GetType().ToString().StartsWith("Microsoft.EntityFrameworkCore.Sqlite"))
                {
                    sql = "DATE('now')";
                    break;
                }
            }
        }

        modelBuilder.Entity<Blog>().Property(b => b.UpdatedDate).HasDefaultValueSql(sql);
        modelBuilder.Entity<Post>().Property(p => p.UpdatedDate).HasDefaultValueSql(sql);
        modelBuilder.Entity<Author>().Property(a => a.UpdatedDate).HasDefaultValueSql(sql);
        modelBuilder.Entity<Category>().Property(c => c.UpdatedDate).HasDefaultValueSql(sql);
        modelBuilder.Entity<Subscriber>().Property(s => s.UpdatedDate).HasDefaultValueSql(sql);
        modelBuilder.Entity<Newsletter>().Property(n => n.UpdatedDate).HasDefaultValueSql(sql);
        modelBuilder.Entity<MailSetting>().Property(n => n.UpdatedDate).HasDefaultValueSql(sql);

        OnModelCreatingPartial(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.NonTransactionalMigrationOperationWarning));
    }


    public override int SaveChanges()
    {
        UpdateDateTrackingFields();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateDateTrackingFields();
        return await base.SaveChangesAsync();
    }

    public virtual DbSet<Author> Authors { get; set; }
    public virtual DbSet<Blog> Blogs { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<WebSite> Domain { get; set; }
    public virtual DbSet<MailSetting> MailSettings { get; set; }
    public virtual DbSet<Menu> Menu { get; set; }
    public virtual DbSet<Newsletter> Newsletters { get; set; }
    public virtual DbSet<PostCategory> PostCategories { get; set; }
    public virtual DbSet<Post> Posts { get; set; }
    public virtual DbSet<Recipe> Recipe { get; set; }
    public virtual DbSet<RecipeCategory> RecipeCategory { get; set; }
    public virtual DbSet<RecipeComment> RecipeComment { get; set; }
    public virtual DbSet<RecipeImage> RecipeImage { get; set; }
    public virtual DbSet<Subscriber> Subscribers { get; set; }
    public virtual DbSet<Keyword> Keywords { get; set; }
    public virtual DbSet<ContentPart> ContentParts { get; set; }

}
