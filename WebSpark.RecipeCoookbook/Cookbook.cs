using iText.Kernel.Events;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Extensions.Logging;
using WebSpark.Core.Interfaces;
using WebSpark.Core.Models;

namespace WebSpark.RecipeCookbook;

public class Cookbook(ILogger<Cookbook> logger, IRecipeService recipeService) : ICookbook
{
    private readonly ILogger<Cookbook> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IRecipeService _recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));

    private static void AddRecipePage(Document doc, RecipeModel? recipe)
    {
        if (recipe == null) return;

        // Add a page break before each recipe
        doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

        // Add recipe title
        Paragraph recipeTitle = new Paragraph(recipe.Name)
            .SetFontSize(16)
            .SetBold()
            .SetMarginTop(10);

        doc.Add(recipeTitle);
        // Add the recipe details
        doc.Add(new Paragraph($"Author: {recipe.AuthorNM}"));
        doc.Add(new Paragraph($"Description: {recipe.Description}"));
        doc.Add(new Paragraph($"Ingredients: {recipe.Ingredients}"));
        doc.Add(new Paragraph($"Instructions: {recipe.Instructions}"));
        doc.Add(new Paragraph($"Servings: {recipe.Servings}"));
        doc.Add(new Paragraph(" ")); // Add space between content
    }

    private static void GenerateCookbookPDF(Document doc, List<RecipeModel> recipes, List<TocEntry> tocEntries)
    {
        // Group recipes by category
        var recipesByCategory = recipes
            .GroupBy(r => r.RecipeCategory.Name)
            .OrderBy(g => g.Key)
            .ToList();

        foreach (var categoryGroup in recipesByCategory)
        {
            // Add category title
            Paragraph categoryTitle = new Paragraph(categoryGroup.Key ?? "Unknown Category")
                .SetFontSize(18)
                .SetBold()
                .SetMarginTop(20);

            doc.Add(categoryTitle);

            // Track the position of the category for TOC
            int categoryStartPage = doc.GetPdfDocument().GetNumberOfPages();
            tocEntries.Add(new TocEntry(categoryGroup.Key, categoryStartPage));

            foreach (var recipe in categoryGroup)
            {
                AddRecipePage(doc, recipe);
            }
            // Add a page break after each category (optional)
            doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
        }
    }

    private static void InsertTableOfContents(Document doc, List<TocEntry> tocEntries)
    {
        // Go back to the first page for the TOC
        doc.GetPdfDocument().SetDefaultPageSize(doc.GetPdfDocument().GetDefaultPageSize());

        // Add TOC title
        Paragraph tocTitle = new Paragraph("Table of Contents")
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(18)
            .SetBold()
            .SetMarginBottom(20);
        doc.Add(tocTitle);

        // Add TOC entries
        foreach (var entry in tocEntries)
        {
            Paragraph tocEntry = new Paragraph(entry.Title)
                .SetFontSize(12)
                .SetMarginBottom(10);

            tocEntry.AddTabStops(new TabStop(500, TabAlignment.RIGHT));
            tocEntry.Add(new Tab());
            tocEntry.Add(entry.PageNumber.ToString());

            doc.Add(tocEntry);
        }

        doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE)); // Add a page break after TOC
    }

    public string? MakeCookbook()
    {
        string outputPath = "Cookbook.pdf";
        try
        {
            PdfWriter writer = new PdfWriter(outputPath);
            PdfDocument pdfDoc = new PdfDocument(writer);

            // Add header and footer event handler
            pdfDoc.AddEventHandler(PdfDocumentEvent.END_PAGE, new HeaderFooterEventHandler());

            Document doc = new Document(pdfDoc);
            List<TocEntry> tocEntries = new List<TocEntry>();

            // Reserve space for TOC
            doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

            // Generate content grouped by category and collect TOC data
            GenerateCookbookPDF(doc, _recipeService.Get().ToList(), tocEntries);

            // Insert the TOC at the beginning
            InsertTableOfContents(doc, tocEntries);

            doc.Close();
            pdfDoc.Close();

            // Return the path to the created PDF
            return outputPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while generating the cookbook PDF.");
            return null; // Return null if there was an error
        }
    }

    // Event handler class for header and footer
    private class HeaderFooterEventHandler : IEventHandler
    {
        public void HandleEvent(Event @event)
        {
            PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
            PdfDocument pdfDoc = docEvent.GetDocument();
            PdfPage page = docEvent.GetPage();
            int pageNumber = pdfDoc.GetPageNumber(page);
            PdfCanvas pdfCanvas = new(page.NewContentStreamBefore(), page.GetResources(), pdfDoc);

            Rectangle pageSize = page.GetPageSize();
            float x = pageSize.GetWidth() / 2;
            float y = pageSize.GetTop() - 20;
            float footerY = pageSize.GetBottom() + 20;

            // Header
            Canvas canvas = new(pdfCanvas, pageSize);
            canvas.ShowTextAligned(new Paragraph("Recipe Cookbook")
                .SetFontSize(12)
                .SetBold(), x, y, TextAlignment.CENTER);

            // Footer with page number
            canvas.ShowTextAligned(new Paragraph($"Page {pageNumber}")
                .SetFontSize(10), x, footerY, TextAlignment.CENTER);

            canvas.Close();
        }
    }

    // Class to store TOC entries
    private class TocEntry(string title, int pageNumber)
    {
        public int PageNumber { get; } = pageNumber;
        public string Title { get; } = title;
    }
}
