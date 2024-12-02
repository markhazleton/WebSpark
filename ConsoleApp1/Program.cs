using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PromptSpark.Domain.Service;

// Setup Configuration
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(); // Specify the type argument explicitly
var configuration = builder.Build();
var serviceCollection = new ServiceCollection();
serviceCollection.AddHttpClient();
var serviceProvider = serviceCollection.BuildServiceProvider();
var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();



MomRecipeRequestService momRecipeRequestService = new(httpClientFactory, configuration);
var recipe = await momRecipeRequestService.GetMomRecipeAIAsync("Maple Leaf - Bourbon, lemon juice, and maple syrup served up in coupe. Get your Canada on, eh?");
Console.WriteLine(recipe.Name);
