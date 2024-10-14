using PromptSpark.Domain.Service;
using PromptSpark.SchemaTest.Utility;
using TriviaSpark.JShow.Models;

Console.WriteLine("Hello, World!");

var service = new JsonSchemaUtility();
// Call the generic method to get JSON Schema for the Person class
string personSchema = service.GenerateJsonSchema<RecipeData>();

// Print the generated schema to the console
Console.WriteLine("JSON Schema for RecipeData class:");
Console.WriteLine(personSchema);

string jShowSchema = service.GenerateJsonSchema<JShowVM>();
Console.WriteLine("JSON Schema for JShowVM class:");
Console.WriteLine(jShowSchema);