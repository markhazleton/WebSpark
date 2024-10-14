using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TriviaSpark.JShow.Data;
using TriviaSpark.JShow.Models;

namespace TriviaSpark.JShow.Service;

public class JShowService : IJShowService
{
    private readonly JShowDbContext _context;
    private readonly string _jsonFilePath;
    private JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public JShowService(JShowDbContext context, JShowConfig config)
    {
        _context = context;
        _jsonFilePath = config.JsonOutputFolder;
    }

    // Helper method to deserialize and set properties on the JShowVM
    private JShowVM DeserializeAndSetProperties(string jsonContent)
    {
        // Deserialize the JSON content into an instance of JShowVM
        JShowVM jShow = JsonSerializer.Deserialize<JShowVM>(jsonContent, _jsonOptions);

        jShow ??= new JShowVM();

        // Loop over the categories and questions to set the Question Category and Round properties
        jShow.Rounds.Jeopardy.Theme = jShow.Theme;
        jShow.Rounds.Jeopardy.JShowId = jShow.Id;
        jShow.Rounds.Jeopardy.Name = "Jeopardy";
        foreach (var category in jShow.Rounds.Jeopardy.Categories)
        {
            category.RoundId = jShow.Rounds.Jeopardy.Id;
            foreach (var question in category.Questions)
            {
                question.ShowNumber = jShow.ShowNumber;
                question.AirDate = jShow.AirDate;
                question.Theme = jShow.Theme;
                question.JShowId = jShow.Id;
                question.CategoryName = category.Name;
                question.CategoryId = category.Id;
                question.RoundName = jShow.Rounds.Jeopardy.Name;
            }
        }

        jShow.Rounds.DoubleJeopardy.Theme = jShow.Theme;
        jShow.Rounds.DoubleJeopardy.Name = "Double Jeopardy";
        jShow.Rounds.DoubleJeopardy.JShowId = jShow.Id;
        foreach (var category in jShow.Rounds.DoubleJeopardy.Categories)
        {
            category.RoundId = jShow.Rounds.DoubleJeopardy.Id;
            foreach (var question in category.Questions)
            {
                question.ShowNumber = jShow.ShowNumber;
                question.AirDate = jShow.AirDate;
                question.Theme = jShow.Theme;
                question.JShowId = jShow.Id;
                question.CategoryName = category.Name;
                question.CategoryId = category.Id;
                question.RoundName = jShow.Rounds.DoubleJeopardy.Name;
            }
        }


        return jShow;
    }

    public async Task<List<JShowVM>> GetJShowsAsync()
    {
        var files = Directory.GetFiles(_jsonFilePath, "JSHOW_*.json");
        var jeopardyShows = (await GetAllJShowsAsync()) as List<JShowVM>;

        foreach (var file in files)
        {
            try
            {
                var jShow = LoadByJsonFile(file);
                if (jShow != null)
                {
                    // Check if the show is already in the database
                    var existingShow = jeopardyShows.FirstOrDefault(s => s.Theme == jShow.Theme);
                    if (existingShow == null)
                    {
                        // If the show does not exist in the database, add it
                        var newEntity = JShowMapper.ToEntity(jShow);
                        await _context.JShows.AddAsync(newEntity);
                        await _context.SaveChangesAsync(); // Save the newly added show to the database
                        jeopardyShows.Add(JShowMapper.ToModel(newEntity));
                    }
                }
            }
            catch (JsonException ex)
            {
                // Log JSON parsing errors and continue processing other files
                Console.WriteLine($"JSON parsing error in file {file}: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log any other errors and continue processing other files
                Console.WriteLine($"Unexpected error in file {file}: {ex.Message}");
            }
        }
        int showNumber = 1;
        foreach (var show in jeopardyShows ?? [])
        {
            show.ShowNumber = showNumber;
            showNumber++;
        }
        return jeopardyShows ?? [];
    }
    public JShowVM? LoadByJsonFile(string filePath)
    {
        try
        {
            // Read the JSON file as a string
            string jsonContent = File.ReadAllText(filePath);
            return DeserializeAndSetProperties(jsonContent);
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"File not found: {filePath}");
            return null;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing JSON: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            return null;
        }
    }

    public JShowVM? LoadByJsonString(string jsonContent)
    {
        try
        {
            return DeserializeAndSetProperties(jsonContent);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing JSON: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            return null;
        }
    }

    public JShowVM ReadJShowFromJsonFile(string filePath)
    {
        try
        {
            // Read the JSON file as a string
            string jsonContent = File.ReadAllText(filePath);

            // Deserialize the JSON content into an instance of JShowVM
            JShowVM jShow = JsonSerializer.Deserialize<JShowVM>(jsonContent, _jsonOptions);

            jShow ??= new JShowVM();

            // Loop over the categories and questions to set the Question Category and Round properties
            jShow.Rounds.DoubleJeopardy.Theme = jShow.Theme;
            jShow.Rounds.Jeopardy.Theme = jShow.Theme;

            foreach (var category in jShow.Rounds.Jeopardy.Categories)
            {
                foreach (var question in category.Questions)
                {
                    question.Theme = jShow.Theme;
                    question.JShowId = jShow.Id;
                    question.CategoryName = category.Name;
                    question.CategoryId = category.Id;
                }
            }
            foreach (var category in jShow.Rounds.DoubleJeopardy.Categories)
            {
                foreach (var question in category.Questions)
                {
                    question.Theme = jShow.Theme;
                    question.JShowId = jShow.Id;
                    question.CategoryName = category.Name;
                    question.CategoryId = category.Id;
                }
            }
            return jShow;
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"File not found: {filePath}");
            return null;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing JSON: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            return null;
        }
    }

    #region JShowVM CRUD Operations

    public async Task<JShowVM> CreateJShowAsync(JShowVM jshow)
    {
        // Check if the theme already exists
        var existingThemeEntity = await _context.JShows
            .Where(j => j.Theme == jshow.Theme)
            .SingleOrDefaultAsync();

        if (existingThemeEntity != null)
        {
            // Theme is already in use
            throw new InvalidOperationException($"The theme '{jshow.Theme}' is already in use. Please choose a different theme.");
        }

        jshow.Id = Guid.NewGuid().ToString();
        jshow.Rounds.Jeopardy.Id = Guid.NewGuid().ToString();
        jshow.Rounds.DoubleJeopardy.Id = Guid.NewGuid().ToString();
        foreach (var category in jshow.Rounds.Jeopardy.Categories)
        {
            category.Id = Guid.NewGuid().ToString();
            category.RoundId = jshow.Rounds.Jeopardy.Id;
            foreach (var question in category.Questions)
            {
                question.Id = Guid.NewGuid().ToString();
                question.CategoryId = category.Id;
                question.RoundName = jshow.Rounds.DoubleJeopardy.Name;
                question.Theme = jshow.Theme;
                question.JShowId = jshow.Id;
                question.CategoryName = category.Name;
                question.CategoryId = category.Id;
            }
        }
        foreach (var category in jshow.Rounds.DoubleJeopardy.Categories)
        {
            category.Id = Guid.NewGuid().ToString();
            category.RoundId = jshow.Rounds.Jeopardy.Id;
            foreach (var question in category.Questions)
            {
                question.Id = Guid.NewGuid().ToString();
                question.CategoryId = category.Id;
                question.RoundName = jshow.Rounds.DoubleJeopardy.Name;
                question.Theme = jshow.Theme;
                question.JShowId = jshow.Id;
                question.CategoryName = category.Name;
                question.CategoryId = category.Id;
            }
        }

        // Convert the view model to an entity
        var entity = JShowMapper.ToEntity(jshow);

        // Add the new entity to the context
        await _context.JShows.AddAsync(entity);

        // Save changes to the database
        await _context.SaveChangesAsync();

        // Return the newly created entity as a view model
        return JShowMapper.ToModel(entity);
    }

    public async Task<JShowVM> GetJShowByIdAsync(string id)
    {
        var entity = await _context.JShows
            .Include(j => j.Rounds)
            .ThenInclude(r => r.Categories)
            .ThenInclude(c => c.Questions)
            .FirstOrDefaultAsync(j => j.Id == id);

        return entity != null ? JShowMapper.ToModel(entity) : null;
    }

    public async Task<IEnumerable<JShowVM>> GetAllJShowsAsync()
    {
        var entities = await _context.JShows
            .Include(j => j.Rounds)
            .ThenInclude(r => r.Categories)
            .ThenInclude(c => c.Questions)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();

        return entities.Select(JShowMapper.ToModel).ToList();
    }

    public async Task<JShowVM> UpdateJShowAsync(JShowVM jshow)
    {
        // Retrieve the existing JShow entity
        var entity = await _context.JShows
            .Include(j => j.Rounds)
            .ThenInclude(r => r.Categories)
            .ThenInclude(c => c.Questions)
            .FirstOrDefaultAsync(j => j.Id == jshow.Id);

        if (entity == null)
            return null;

        // Check if the new theme is already in use by another entity
        var existingThemeEntity = await _context.JShows
            .Where(j => j.Theme == jshow.Theme && j.Id != jshow.Id)
            .SingleOrDefaultAsync();

        if (existingThemeEntity != null)
        {
            // Theme is already in use by another JShowEntity
            throw new InvalidOperationException($"The theme '{jshow.Theme}' is already in use by another show.");
        }

        // Update entity properties
        entity.ShowNumber = jshow.ShowNumber;
        entity.AirDate = jshow.AirDate;
        entity.Theme = jshow.Theme;
        entity.Description = jshow.Description;
        entity.Type = jshow.Type;
        // Update more properties as necessary

        // Update the entity in the context
        _context.JShows.Update(entity);

        // Save changes to the database
        await _context.SaveChangesAsync();

        // Return the updated model
        return JShowMapper.ToModel(entity);
    }


    public async Task<bool> DeleteJShowAsync(string id)
    {
        var entity = await _context.JShows.FindAsync(id);
        if (entity == null)
            return false;

        _context.JShows.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region JShowRoundEntity CRUD Operations

    public async Task<RoundVM> CreateRoundAsync(RoundVM round)
    {
        JShowRoundEntity dbRound = JShowMapper.ToEntity(round);
        await _context.JShowRounds.AddAsync(dbRound);
        await _context.SaveChangesAsync();
        return JShowMapper.ToModel(dbRound);
    }

    public async Task<RoundVM> GetRoundByIdAsync(string id)
    {
        var dbRound = await _context.JShowRounds
            .Include(r => r.Categories)
            .ThenInclude(c => c.Questions)
            .FirstOrDefaultAsync(r => r.Id == id) ?? new JShowRoundEntity();
        return JShowMapper.ToModel(dbRound);
    }

    public async Task<IEnumerable<RoundVM>> GetRoundsByJShowIdAsync(string showId)
    {
        var dbList = await _context.JShowRounds
            .Where(r => r.JShowId == showId)
            .Include(r => r.Categories)
            .ThenInclude(c => c.Questions)
            .ToListAsync();
        return dbList.Select(JShowMapper.ToModel).ToList();
    }

    public async Task<RoundVM> UpdateRoundAsync(RoundVM round)
    {
        var entity = await _context.JShowRounds.FindAsync(round.Id);
        if (entity == null)
            return null;

        // Update entity properties
        entity.Name = round.Name;
        entity.Theme = round.Theme;
        // Update more properties as necessary

        _context.JShowRounds.Update(entity);
        await _context.SaveChangesAsync();
        return JShowMapper.ToModel(entity);
    }

    public async Task<bool> DeleteRoundAsync(string id)
    {
        var entity = await _context.JShowRounds.FindAsync(id);
        if (entity == null)
            return false;

        _context.JShowRounds.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region CategoryVM CRUD Operations

    public async Task<CategoryVM> CreateCategoryAsync(CategoryVM category)
    {
        // Check if a category with the same RoundId and Name already exists
        var existingCategory = await _context.Categories
            .Where(c => c.RoundId == category.RoundId && c.Name == category.Name)
            .SingleOrDefaultAsync();

        if (existingCategory != null)
        {
            // Throw an exception or return a specific result indicating a conflict
            throw new InvalidOperationException($"A category with the name '{category.Name}' already exists for this round.");
        }
        // Convert the view model to an entity
        CategoryEntity dbCategory = JShowMapper.ToEntity(category);

        // Add the new category to the context
        await _context.Categories.AddAsync(dbCategory);

        // Save changes to the database
        await _context.SaveChangesAsync();

        // Return the newly created category
        return JShowMapper.ToModel(dbCategory);
    }

    public async Task<CategoryVM> GetCategoryByIdAsync(string id)
    {
        CategoryEntity dbCategory = await _context.Categories
            .Include(c => c.Questions)
            .FirstOrDefaultAsync(c => c.Id == id) ?? new CategoryEntity();

        return JShowMapper.ToModel(dbCategory);
    }

    public async Task<IEnumerable<CategoryVM>> GetCategoriesByRoundIdAsync(string roundId)
    {
        var dbList = await _context.Categories
            .Where(c => c.RoundId == roundId)
            .Include(c => c.Questions)
            .ToListAsync();
        return dbList.Select(JShowMapper.ToModel).ToList();
    }

    public async Task<CategoryVM> UpdateCategoryAsync(CategoryVM category)
    {
        // Find the existing category entity in the database
        var entity = await _context.Categories.FindAsync(category.Id);
        if (entity == null)
            return category;

        // Check if a different category with the same RoundId and Name already exists
        var existingCategory = await _context.Categories
            .Where(c => c.RoundId == entity.RoundId && c.Name == category.Name && c.Id != category.Id)
            .SingleOrDefaultAsync();

        if (existingCategory != null)
        {
            // Throw an exception or return a specific result indicating a conflict
            throw new InvalidOperationException($"A category with the name '{category.Name}' already exists for this round.");
        }

        // Update entity properties
        entity.Name = category.Name;
        // Update more properties as necessary
        entity.ModifiedDate = DateTime.UtcNow; // Optional: update modification timestamp

        // Update the entity in the context
        _context.Categories.Update(entity);

        // Save changes to the database
        await _context.SaveChangesAsync();

        return JShowMapper.ToModel(entity);

    }


    public async Task<bool> DeleteCategoryAsync(string id)
    {
        var entity = await _context.Categories.FindAsync(id);
        if (entity == null)
            return false;

        _context.Categories.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region QuestionVM CRUD Operations

    public async Task<QuestionVM> CreateQuestionAsync(QuestionVM question)
    {
        // Check if a question with the same CategoryId and Value already exists
        var existingQuestionWithValue = await _context.Questions
            .Where(q => q.CategoryId == question.CategoryId && q.Value == question.Value)
            .SingleOrDefaultAsync();

        if (existingQuestionWithValue != null)
        {
            // Throw an exception or return a specific result indicating a conflict
            throw new InvalidOperationException($"A question with value '{question.Value}' already exists in this category.");
        }

        // Check if a question with the same CategoryId and QuestionText already exists
        var existingQuestionWithText = await _context.Questions
            .Where(q => q.CategoryId == question.CategoryId && q.QuestionText == question.QuestionText)
            .SingleOrDefaultAsync();

        if (existingQuestionWithText != null)
        {
            // Throw an exception or return a specific result indicating a conflict
            throw new InvalidOperationException($"A question with the same text already exists in this category.");
        }
        // Convert the view model to an entity
        QuestionEntity dbQuestion = JShowMapper.ToEntity(question);

        // Add the new question to the context
        await _context.Questions.AddAsync(dbQuestion);

        // Save changes to the database
        await _context.SaveChangesAsync();

        // Return the newly created question
        return question;
    }
    public async Task<List<QuestionVM>> GetQuestionVMsAsync()
    {
        var entities = await _context
            .Questions
            .Include(q => q.Category).ThenInclude(c => c.JShowRound).ThenInclude(r => r.JShow)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();
        return entities.Select(JShowMapper.ToModel).ToList();
    }
    public async Task<QuestionVM> GetQuestionByIdAsync(string id)
    {
        QuestionEntity dbQueston = await _context
            .Questions
            .Include(QuestionVM => QuestionVM.Category).ThenInclude(CategoryVM => CategoryVM.JShowRound).ThenInclude(CategoryVM => CategoryVM.JShow)
            .Where(w => w.Id == id)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync() ?? new QuestionEntity();
        return JShowMapper.ToModel(dbQueston);
    }

    public async Task<IEnumerable<QuestionVM>> GetQuestionsByCategoryIdAsync(string categoryId)
    {
        var dbList = await _context.Questions
            .Where(q => q.CategoryId == categoryId)
            .ToListAsync();
        return dbList.Select(JShowMapper.ToModel).ToList();
    }
    public async Task<QuestionVM> UpdateQuestionAsync(QuestionVM question)
    {
        // Find the existing question entity in the database
        var entity = await _context.Questions.FindAsync(question.Id);
        if (entity == null)
            return question;

        // Check if another question with the same CategoryId and Value already exists
        var existingQuestionWithValue = await _context.Questions
            .Where(q => q.CategoryId == entity.CategoryId && q.Value == question.Value && q.Id != question.Id)
            .SingleOrDefaultAsync();

        if (existingQuestionWithValue != null)
        {
            // Throw an exception or return a specific result indicating a conflict
            throw new InvalidOperationException($"A question with value '{question.Value}' already exists in this category.");
        }

        // Check if another question with the same CategoryId and QuestionText already exists
        var existingQuestionWithText = await _context.Questions
            .Where(q => q.CategoryId == entity.CategoryId && q.QuestionText == question.QuestionText && q.Id != question.Id)
            .SingleOrDefaultAsync();

        if (existingQuestionWithText != null)
        {
            // Throw an exception or return a specific result indicating a conflict
            throw new InvalidOperationException($"A question with the same text already exists in this category.");
        }

        // Update entity properties
        entity.QuestionText = question.QuestionText;
        entity.Answer = question.Answer;
        entity.Value = question.Value;
        entity.Theme = question.Theme;
        // Update more properties as necessary
        entity.ModifiedDate = DateTime.UtcNow; // Optional: update modification timestamp

        // Update the entity in the context
        _context.Questions.Update(entity);

        // Save changes to the database
        await _context.SaveChangesAsync();

        return JShowMapper.ToModel(entity);
    }


    public async Task<bool> DeleteQuestionAsync(string id)
    {
        var entity = await _context.Questions.FindAsync(id);
        if (entity == null)
            return false;

        _context.Questions.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion
}
public static class JsonElementExtensions
{

    /// <summary>
    /// Safely converts a JsonElement to a Guid. Returns null if the conversion fails.
    /// </summary>
    public static Guid GetGuid(this JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String && Guid.TryParse(element.GetString(), out var guid))
        {
            return guid;
        }
        return Guid.Empty;
    }
    /// <summary>
    /// Safely gets a property from a JsonElement. Returns null if the property does not exist.
    /// </summary>
    public static JsonElement? GetPropertySafe(this JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var value))
        {
            return value;
        }
        return null;
    }
}