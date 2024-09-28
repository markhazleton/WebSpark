using TriviaSpark.JShow.Data;
using TriviaSpark.JShow.Models;

namespace TriviaSpark.JShow.Service;

public interface IJShowService
{
    Task<List<JShowVM>> GetJShowsAsync();
    // JShowVM CRUD operations
    Task<JShowVM> CreateJShowAsync(JShowVM jshow);
    Task<JShowVM> GetJShowByIdAsync(string id);
    Task<IEnumerable<JShowVM>> GetAllJShowsAsync();
    Task<JShowVM> UpdateJShowAsync(JShowVM jshow);
    Task<bool> DeleteJShowAsync(string id);

    // RoundVM CRUD operations
    Task<RoundVM> CreateRoundAsync(RoundVM round);
    Task<RoundVM> GetRoundByIdAsync(string id);
    Task<RoundVM> UpdateRoundAsync(RoundVM round);
    Task<bool> DeleteRoundAsync(string id);

    // CategoryEntity CRUD operations
    Task<CategoryVM> CreateCategoryAsync(CategoryVM category);
    Task<CategoryVM> GetCategoryByIdAsync(string id);
    Task<IEnumerable<CategoryVM>> GetCategoriesByRoundIdAsync(string roundId);
    Task<CategoryVM> UpdateCategoryAsync(CategoryVM category);
    Task<bool> DeleteCategoryAsync(string id);

    // QuestionEntity CRUD operations
    Task<QuestionVM> CreateQuestionAsync(QuestionVM question);
    Task<QuestionVM> GetQuestionByIdAsync(string id);
    Task<IEnumerable<QuestionVM>> GetQuestionsByCategoryIdAsync(string categoryId);
    Task<QuestionVM> UpdateQuestionAsync(QuestionVM question);
    Task<bool> DeleteQuestionAsync(string id);


}

