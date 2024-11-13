namespace ConsoleApp1.Service;

// Define the RecipeData record
public record RecipeData(
    string Name,
    string Description,
    string Category,
    int Servings,
    List<string> Ingredients,
    List<string> Instructions,
    List<string> SEO_Keywords
);