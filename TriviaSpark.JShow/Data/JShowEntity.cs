using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TriviaSpark.JShow.Models;

namespace TriviaSpark.JShow.Data;

public abstract class BaseEntity
{
    [Key]
    public string Id { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }

    [Required]
    public DateTime? ModifiedDate { get; set; }

    [Required]
    public string CreatedBy { get; set; }

    [Required]
    public string ModifiedBy { get; set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid().ToString();
        CreatedDate = DateTime.UtcNow;
        ModifiedDate = DateTime.UtcNow;
        ModifiedBy = "System";
        CreatedBy = "System";
    }
}
public class JShowEntity : BaseEntity
{
    public int ShowNumber { get; set; }

    public string AirDate { get; set; }

    [Required] // Ensures that the Theme is not null or empty
    [StringLength(100, ErrorMessage = "Theme cannot be longer than 100 characters.")] // Limits the length of the Theme
    public string Theme { get; set; }

    public string Description { get; set; }

    public ICollection<JShowRoundEntity> Rounds { get; set; }
}

public class JShowRoundEntity : BaseEntity
{
    [ForeignKey("JShowEntity")]
    public string JShowId { get; set; }

    public string Name { get; set; }
    public string Theme { get; set; }
    public ICollection<CategoryEntity> Categories { get; set; }
    public JShowEntity JShow { get; set; }
}
public class CategoryEntity : BaseEntity
{
    [Required] // Ensures RoundId is not null
    [ForeignKey("JShowRoundEntity")]
    public string RoundId { get; set; }

    [Required] // Ensures Name is not null or empty
    [StringLength(100, ErrorMessage = "Category name cannot be longer than 100 characters.")]
    public string Name { get; set; }
    public ICollection<QuestionEntity> Questions { get; set; }
    public JShowRoundEntity JShowRound { get; set; }
}
public class QuestionEntity : BaseEntity
{
    [ForeignKey("CategoryEntity")]
    [Required] // Ensures CategoryId is not null
    public string CategoryId { get; set; }

    [Required] // Ensures Value is not null
    public int Value { get; set; }

    [Required] // Ensures QuestionText is not null or empty
    [StringLength(500, ErrorMessage = "Question text cannot be longer than 500 characters.")]
    public string QuestionText { get; set; }
    public string Answer { get; set; }
    public string Theme { get; set; }
    public CategoryEntity Category { get; set; }
}
public class JShowDbContextFactory : IDesignTimeDbContextFactory<JShowDbContext>
{
    public JShowDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<JShowDbContext>();

        var connectionStringBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = "c:\\websites\\WebSpark\\JShow.db"
        };
        var connectionString = connectionStringBuilder.ToString();
        Console.WriteLine($"Connection String: {connectionString}"); // Log the connection string

        optionsBuilder.UseSqlite(connectionString);

        return new JShowDbContext(optionsBuilder.Options);
    }
}


public class JShowDbContext : DbContext
{
    public JShowDbContext(DbContextOptions<JShowDbContext> options) : base(options) { }
    public DbSet<JShowEntity> JShows { get; set; }
    public DbSet<JShowRoundEntity> JShowRounds { get; set; }
    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<QuestionEntity> Questions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<JShowEntity>()
            .HasMany(j => j.Rounds)
            .WithOne(r => r.JShow)
            .HasForeignKey(r => r.JShowId);

        modelBuilder.Entity<JShowEntity>()
            .HasIndex(j => j.Theme)
            .IsUnique();

        modelBuilder.Entity<JShowRoundEntity>()
            .HasMany(r => r.Categories)
            .WithOne(c => c.JShowRound)
            .HasForeignKey(c => c.RoundId);

        modelBuilder.Entity<CategoryEntity>()
            .HasMany(c => c.Questions)
            .WithOne(q => q.Category)
            .HasForeignKey(q => q.CategoryId);
        // Define composite unique index for CategoryEntity on RoundId and Name
        modelBuilder.Entity<CategoryEntity>()
            .HasIndex(c => new { c.RoundId, c.Name })
            .IsUnique()
            .HasDatabaseName("IX_Category_RoundId_Name"); // Optional: custom index name
                                                          // Composite unique index on CategoryId and Value for QuestionEntity
        modelBuilder.Entity<QuestionEntity>()
            .HasIndex(q => new { q.CategoryId, q.Value })
            .IsUnique()
            .HasDatabaseName("IX_Question_CategoryId_Value"); // Optional: custom index name

        // Composite unique index on CategoryId and QuestionText for QuestionEntity
        modelBuilder.Entity<QuestionEntity>()
            .HasIndex(q => new { q.CategoryId, q.QuestionText })
            .IsUnique()
            .HasDatabaseName("IX_Question_CategoryId_QuestionText"); // Optional: custom index name

    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            if (entry.State == EntityState.Added)
            {
                entity.CreatedDate = DateTime.UtcNow;
                entity.CreatedBy = "System"; // Replace with the current user if applicable

                if (entity is JShowEntity jShowEntity)
                {
                    // Check for existing theme before adding
                    var existingTheme = await JShows
                        .Where(j => j.Theme == jShowEntity.Theme)
                        .SingleOrDefaultAsync();

                    if (existingTheme != null)
                    {
                        throw new InvalidOperationException($"The theme '{jShowEntity.Theme}' already exists.");
                    }
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.ModifiedDate = DateTime.UtcNow;
                entity.ModifiedBy = "System"; // Replace with the current user if applicable

                if (entity is JShowEntity jShowEntity)
                {
                    // Check for existing theme before modifying
                    var existingTheme = await JShows
                        .Where(j => j.Theme == jShowEntity.Theme && j.Id != jShowEntity.Id)
                        .SingleOrDefaultAsync(cancellationToken: cancellationToken);

                    if (existingTheme != null)
                    {
                        throw new InvalidOperationException($"The theme '{jShowEntity.Theme}' already exists.");
                    }
                }
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
public static class JShowMapper
{
    // Maps JShowVM model to JShowEntity
    public static JShowEntity ToEntity(JShowVM jshow)
    {
        return new JShowEntity
        {
            Id = jshow.Id,
            ShowNumber = jshow.ShowNumber,
            AirDate = jshow.AirDate,
            Theme = jshow.Theme,
            Description = jshow.Description,
            CreatedDate = DateTime.UtcNow, // Or map from model if available
            CreatedBy = "System", // Or map from model if available
            Rounds = MapRoundsToEntity(jshow.Rounds, jshow.Id)
        };
    }

    // Maps Rounds model to list of JShowRoundEntity
    private static List<JShowRoundEntity> MapRoundsToEntity(Rounds rounds, string jshowId)
    {
        var roundEntities = new List<JShowRoundEntity>();

        // Map Jeopardy round
        if (rounds.Jeopardy != null)
        {
            roundEntities.Add(new JShowRoundEntity
            {
                JShowId = jshowId,
                Name = "Jeopardy",
                Theme = rounds.Jeopardy.Theme,
                Categories = MapCategoriesToEntity(rounds.Jeopardy.Categories)
            });
        }

        // Map Double Jeopardy round
        if (rounds.DoubleJeopardy != null)
        {
            roundEntities.Add(new JShowRoundEntity
            {
                JShowId = jshowId,
                Name = "Double Jeopardy",
                Theme = rounds.DoubleJeopardy.Theme,
                Categories = MapCategoriesToEntity(rounds.DoubleJeopardy.Categories)
            });
        }

        // Map Final Jeopardy round
        if (rounds.FinalJeopardy != null)
        {
            roundEntities.Add(new JShowRoundEntity
            {
                JShowId = jshowId,
                Name = "Final Jeopardy",
                Theme = rounds.FinalJeopardy.Category, // Using Category as the theme for Final Jeopardy
                Categories =
                    [
                        new CategoryEntity
                        {
                            Name = rounds.FinalJeopardy.Category,
                            Questions =
                            [
                                new QuestionEntity
                                {
                                    QuestionText = rounds.FinalJeopardy.QuestionText,
                                    Answer = rounds.FinalJeopardy.Answer,
                                    Value = 0, // No specific value for Final Jeopardy question
                                    Theme = rounds.FinalJeopardy.Category
                                }
                            ]
                        }
                    ]
            });
        }

        return roundEntities;
    }

    // Maps list of Category model to list of CategoryEntity
    private static List<CategoryEntity> MapCategoriesToEntity(List<CategoryVM> categories)
    {
        return categories.Select(category => new CategoryEntity
        {
            Name = category.Name,
            Questions = MapQuestionsToEntity(category.Questions)
        }).ToList();
    }

    // Maps list of Question model to list of QuestionEntity
    private static List<QuestionEntity> MapQuestionsToEntity(List<QuestionVM> questions)
    {
        return questions.Select(question => new QuestionEntity
        {
            Id = question.Id,
            QuestionText = question.QuestionText,
            Answer = question.Answer,
            Value = question.Value,
            Theme = question.Theme
        }).ToList();
    }

    // Maps JShowEntity to JShowVM model
    public static JShowVM ToModel(JShowEntity entity)
    {
        return new JShowVM
        {
            Id = entity.Id,
            ShowNumber = entity.ShowNumber,
            AirDate = entity.AirDate,
            Theme = entity.Theme,
            Description = entity.Description,
            Rounds = MapRoundsToModel(entity.Rounds)
        };
    }

    // Maps list of JShowRoundEntity to Rounds model
    private static Rounds MapRoundsToModel(ICollection<JShowRoundEntity> roundEntities)
    {
        var rounds = new Rounds();

        // Map Jeopardy round
        var jeopardyRound = roundEntities.FirstOrDefault(r => r.Name == "Jeopardy");
        if (jeopardyRound != null)
        {
            rounds.Jeopardy = new RoundVM
            {
                Theme = jeopardyRound.Theme,
                Categories = MapCategoriesToModel(jeopardyRound.Categories)
            };
        }

        // Map Double Jeopardy round
        var doubleJeopardyRound = roundEntities.FirstOrDefault(r => r.Name == "Double Jeopardy");
        if (doubleJeopardyRound != null)
        {
            rounds.DoubleJeopardy = new RoundVM
            {
                Theme = doubleJeopardyRound.Theme,
                Categories = MapCategoriesToModel(doubleJeopardyRound.Categories)
            };
        }

        // Map Final Jeopardy round
        var finalJeopardyRound = roundEntities.FirstOrDefault(r => r.Name == "Final Jeopardy");
        if (finalJeopardyRound != null && finalJeopardyRound.Categories.Any())
        {
            var finalCategory = finalJeopardyRound.Categories.First();
            if (finalCategory.Questions.Any())
            {
                var finalQuestion = finalCategory.Questions.First();
                rounds.FinalJeopardy = new FinalJeopardy
                {
                    Category = finalCategory.Name,
                    QuestionText = finalQuestion.QuestionText,
                    Answer = finalQuestion.Answer
                };
            }
        }

        return rounds;
    }

    // Maps list of CategoryEntity to list of Category model
    private static List<CategoryVM> MapCategoriesToModel(ICollection<CategoryEntity> categoryEntities)
    {
        return categoryEntities.Select(categoryEntity => new CategoryVM
        {
            Name = categoryEntity.Name,
            Questions = MapQuestionsToModel(categoryEntity.Questions)
        }).ToList();
    }

    // Maps list of QuestionEntity to list of Question model
    private static List<QuestionVM> MapQuestionsToModel(ICollection<QuestionEntity> questionEntities)
    {


        return questionEntities.Select(questionEntity => new QuestionVM
        {
            Id = questionEntity.Id,
            QuestionText = questionEntity.QuestionText,
            Answer = questionEntity.Answer,
            Value = questionEntity.Value,
            Theme = questionEntity.Theme,
            CategoryId = questionEntity.CategoryId,
            CategoryName = questionEntity.Category?.Name,
            JShowId = questionEntity.Category?.JShowRound?.JShow?.Id
        }).ToList();
    }

    public static JShowRoundEntity ToEntity(RoundVM round)
    {
        return new JShowRoundEntity
        {
            Id = round.Id,
            JShowId = round.JShowId,
            Name = round.Name,
            Theme = round.Theme,
            Categories = round.Categories.Select(c => new CategoryEntity
            {
                Name = c.Name,
                Questions = c.Questions.Select(q => new QuestionEntity
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Answer = q.Answer,
                    Value = q.Value,
                    Theme = q.Theme
                }).ToList()
            }).ToList()
        };

    }

    public static RoundVM ToModel(JShowRoundEntity? dbRound)
    {
        if (dbRound == null)
        {
            return new RoundVM();
        }
        return new RoundVM
        {
            Id = dbRound.Id,
            JShowId = dbRound.JShowId,
            Name = dbRound.Name,
            Theme = dbRound.Theme,
            Categories = dbRound.Categories.Select(c => new CategoryVM
            {
                Name = c.Name,
                Questions = c.Questions.Select(q => new QuestionVM
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Answer = q.Answer,
                    Value = q.Value,
                    Theme = q.Theme
                }).ToList()
            }).ToList()
        };
    }

    public static CategoryEntity ToEntity(CategoryVM category)
    {
        return new CategoryEntity
        {
            Id = category.Id,
            RoundId = category.RoundId,
            Name = category.Name,
            Questions = category.Questions.Select(q => new QuestionEntity
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                Answer = q.Answer,
                Value = q.Value,
                Theme = q.Theme
            }).ToList()
        };
    }

    public static CategoryVM ToModel(CategoryEntity dbCategory)
    {
        return new CategoryVM
        {
            Id = dbCategory.Id,
            RoundId = dbCategory.RoundId,
            Name = dbCategory.Name,
            Questions = dbCategory.Questions.Select(q => new QuestionVM
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                Answer = q.Answer,
                Value = q.Value,
                Theme = q.Theme
            }).ToList()
        };
    }

    public static QuestionEntity ToEntity(QuestionVM question)
    {
        return new QuestionEntity
        {
            Id = question.Id,
            CategoryId = question.CategoryId,
            QuestionText = question.QuestionText,
            Answer = question.Answer,
            Value = question.Value,
            Theme = question.Theme
        };
    }

    public static QuestionVM ToModel(QuestionEntity dbQueston)
    {
        return new QuestionVM
        {
            Id = dbQueston.Id,
            CategoryId = dbQueston.CategoryId,
            QuestionText = dbQueston.QuestionText,
            Answer = dbQueston.Answer,
            Value = dbQueston.Value,
            Theme = dbQueston.Theme
        };
    }
}