using ControlSpark.RecipeManager.Interfaces;
using ControlSpark.RecipeManager.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Extensions.Logging;

namespace ControlSpark.RecipeCookbook;

public class Cookbook : ICookbook
{
    private readonly ILogger<Cookbook> _logger;
    private readonly IRecipeService _RecipeService;

    public Cookbook(ILogger<Cookbook> logger, IRecipeService RecipeService)
    {
        // set up logger and recipe service
        _logger = logger;
        _RecipeService = RecipeService;
    }
    public void MakeCookbook()
    {
        string outputPath = "Cookbook.pdf";
        var recipes = _RecipeService.Get().ToList();
        PdfDocument pdfDoc = new(new PdfWriter(outputPath));
        Document doc = new(pdfDoc);
        GenerateCookbookPDF(doc, recipes);
        doc.Close();
    }
    private void GenerateCookbookPDF(Document doc, List<RecipeModel> recipes)
    {
        Table table = new(UnitValue.CreatePercentArray(new float[] { 5, 1 }));
        table.SetWidth(UnitValue.CreatePercentValue(25));
        table.SetTextAlignment(TextAlignment.LEFT);
        // loop through recipes
        foreach (RecipeModel recipe in recipes)
        {
            table.AddCell(new Cell().Add(new Paragraph(recipe.Name)).SetBorder(Border.NO_BORDER));
            table.AddCell(new Cell().Add(new Paragraph(recipe.RecipeCategory.Name ?? "unknown")).SetBorder(Border.NO_BORDER));
        }
        doc.Add(table);
    }

}
