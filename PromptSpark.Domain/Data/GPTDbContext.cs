using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PromptSpark.Domain.Data;


/// <summary>
/// Represents the database context for PromptSpark.
/// </summary>
public class GPTDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GPTDbContext"/> class.
    /// </summary>
    /// <param name="options">The options for the database context.</param>
    public GPTDbContext(DbContextOptions<GPTDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the DbSet for GPTDefinitionResponse entities.
    /// </summary>
    public DbSet<GPTDefinitionResponse> DefinitionResponses { get; set; }

    /// <summary>
    /// Gets or sets the DbSet for GPTUserPrompt entities.
    /// </summary>
    public DbSet<GPTUserPrompt> Chats { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public DbSet<GPTDefinitionType> DefinitionTypes { get; set; }

    /// <summary>
    /// Gets or sets the DbSet for GPTDefinition entities.
    /// </summary>
    public DbSet<GPTDefinition> Definitions { get; set; }
    /// <summary>
    /// List of all the persona traits
    /// </summary>

    /// <summary>
    /// Configures the model for the database context.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ensure TestPrompt is unique
        modelBuilder.Entity<GPTUserPrompt>()
            .HasIndex(c => new { c.UserPrompt, c.DefinitionType})
             .IsUnique();

        // Relationship between GPTUserPrompt and GPTDefinitionResponse
        modelBuilder.Entity<GPTUserPrompt>()
            .HasMany(r => r.GPTResponses)
            .WithOne(s => s.Response)
            .HasForeignKey(s => s.ResponseId);

        // Relationship between GPTDefinitionResponse and GPTUserPrompt
        modelBuilder.Entity<GPTDefinitionResponse>()
            .HasOne(s => s.Response)
            .WithMany(r => r.GPTResponses)
            .HasForeignKey(s => s.ResponseId);

        // Relationship between GPTDefinitionResponse and GPTDefinition
        modelBuilder.Entity<GPTDefinitionResponse>()
            .HasOne(s => s.Definition)
            .WithMany(d => d.GPTResponses)
            .HasForeignKey(s => s.DefinitionId);

        modelBuilder.Entity<GPTUserPrompt>()
            .HasIndex(p => p.UserPrompt) // Specify the property to be indexed
            .IsUnique(); // Enforce uniqueness on the index

        modelBuilder.Entity<GPTDefinition>()
            .HasIndex(p => p.GPTName) // Specify the property to be indexed
            .IsUnique(); // Enforce uniqueness on the index

        modelBuilder.Entity<GPTDefinitionType>()
            .HasIndex(p => p.DefinitionType) // Specify the property to be indexed
            .IsUnique(); // Enforce uniqueness on the index
    }
}
