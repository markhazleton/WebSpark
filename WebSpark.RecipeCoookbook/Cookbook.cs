using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
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

    private static void AddRecipePage(Document doc, RecipeModel? recipe, Style recipeTitleStyle, Style bodyStyle)
    {
        if (recipe == null) return;

        // Add a page break before each recipe
        doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

        // Add recipe title
        Paragraph recipeTitle = new Paragraph(recipe.Name)
            .AddStyle(recipeTitleStyle);

        doc.Add(recipeTitle);

        // Add the recipe details with styled paragraphs
        doc.Add(new Paragraph($"Source: {recipe.AuthorNM}").AddStyle(bodyStyle));
        doc.Add(new Paragraph($"{recipe.Description}").AddStyle(bodyStyle));
        doc.Add(new Paragraph($"Ingredients: {recipe.Ingredients}").AddStyle(bodyStyle));
        doc.Add(new Paragraph($"Instructions: {recipe.Instructions}").AddStyle(bodyStyle));
        doc.Add(new Paragraph($"Servings: {recipe.Servings}").AddStyle(bodyStyle));
        doc.Add(new Paragraph(" ").AddStyle(bodyStyle)); // Add space between content
    }

    private static void GenerateCookbookPDF(Document doc, List<RecipeModel> recipes, List<TocEntry> tocEntries, Style categoryTitleStyle, Style recipeTitleStyle, Style bodyStyle)
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
                .AddStyle(categoryTitleStyle);

            doc.Add(categoryTitle);

            // Track the position of the category for TOC
            int categoryStartPage = doc.GetPdfDocument().GetNumberOfPages();
            tocEntries.Add(new TocEntry(categoryGroup.Key, categoryStartPage));

            foreach (var recipe in categoryGroup)
            {
                AddRecipePage(doc, recipe, recipeTitleStyle, bodyStyle);
            }

            // Add a page break after each category (optional)
            doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
        }
    }

    private static void InsertTableOfContents(Document doc, List<TocEntry> tocEntries, Style tocTitleStyle, Style tocEntryStyle)
    {
        // Add TOC title
        Paragraph tocTitle = new Paragraph("Table of Contents")
            .AddStyle(tocTitleStyle);
        doc.Add(tocTitle);

        // Add TOC entries
        foreach (var entry in tocEntries)
        {
            Paragraph tocEntry = new Paragraph(entry.Title)
                .AddStyle(tocEntryStyle);

            tocEntry.AddTabStops(new TabStop(500, TabAlignment.RIGHT));
            tocEntry.Add(new Tab());
            tocEntry.Add(entry.PageNumber.ToString());

            doc.Add(tocEntry);
        }

        doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE)); // Add a page break after TOC
    }

    private static void AddTitlePage(Document doc, string name, string description, Style titleStyle, Style descStyle)
    {
        Paragraph title = new Paragraph(name)
            .AddStyle(titleStyle);

        Paragraph desc = new Paragraph(description)
            .AddStyle(descStyle);

        doc.Add(title);
        doc.Add(desc);

        // Add a page break after the title page
        doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
    }

    public string? MakeCookbook(string outputPath, string name, string description)
    {
        try
        {
            PdfWriter writer = new PdfWriter(outputPath);
            PdfDocument pdfDoc = new PdfDocument(writer);

            // Add header and footer event handler
            pdfDoc.AddEventHandler(PdfDocumentEvent.END_PAGE, new HeaderFooterEventHandler());

            Document doc = new Document(pdfDoc);
            List<TocEntry> tocEntries = new List<TocEntry>();

            // Define styles
            Style titleStyle = new Style()
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD))
                .SetFontSize(36)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(ColorConstants.BLACK);

            Style descStyle = new Style()
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(ColorConstants.DARK_GRAY);

            Style categoryTitleStyle = new Style()
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD))
                .SetFontSize(24)
                .SetFontColor(ColorConstants.BLUE)
                .SetMarginTop(20);

            Style recipeTitleStyle = new Style()
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(16)
                .SetFontColor(ColorConstants.DARK_GRAY)
                .SetMarginTop(10);

            Style bodyStyle = new Style()
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                .SetFontSize(12)
                .SetFontColor(ColorConstants.BLACK);

            Style tocTitleStyle = new Style()
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD))
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(ColorConstants.BLACK)
                .SetMarginBottom(20);

            Style tocEntryStyle = new Style()
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                .SetFontSize(12)
                .SetFontColor(ColorConstants.BLACK)
                .SetMarginBottom(10);

            // Add title page
            AddTitlePage(doc, name, description, titleStyle, descStyle);

            // Reserve space for TOC
            doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

            // Generate content grouped by category and collect TOC data
            GenerateCookbookPDF(doc, _recipeService.Get().ToList(), tocEntries, categoryTitleStyle, recipeTitleStyle, bodyStyle);

            // Insert the TOC at the beginning
            InsertTableOfContents(doc, tocEntries, tocTitleStyle, tocEntryStyle);

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
