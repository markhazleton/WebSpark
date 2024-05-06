using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebSpark.Prompt.Data;

// Build the configuration object from `appsettings.json`
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Create an instance of the DbContext using the configuration
var optionsBuilder = new DbContextOptionsBuilder<GPTDbContext>();

optionsBuilder.UseSqlite(configuration.GetConnectionString("PromptSparkConnection"));

using GPTDbContext dbContext  = new(optionsBuilder.Options);

// Retrieve and print data from the `Products` table
var definitionTypes = dbContext.DefinitionTypes.ToList();

Console.WriteLine("ID | Name ");
Console.WriteLine("------------------------");

foreach (var definitionType in definitionTypes)
{
    Console.WriteLine($"{definitionType.DefinitionType} | {definitionType.Description}");
}

Console.WriteLine("Done.");
