using iText.Commons.Actions;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Renderer;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using WebSpark.Core.Interfaces;
using WebSpark.Core.Models;

namespace WebSpark.RecipeCookbook;

/// <summary>
/// Handles the creation of recipe cookbooks in PDF format
/// </summary>
public class Cookbook : ICookbook
{
    private readonly ILogger<Cookbook> _logger;
    private readonly IRecipeService _recipeService;

    // PDF Styling constants
    private const string PRIMARY_FONT = StandardFonts.HELVETICA;
    private const string HEADING_FONT = StandardFonts.TIMES_BOLD;
    private const float BODY_FONT_SIZE = 11f;
    private const float TITLE_FONT_SIZE = 36f;
    private const float CATEGORY_FONT_SIZE = 24f;
    private const float RECIPE_TITLE_FONT_SIZE = 18f;
    private const float SECTION_TITLE_FONT_SIZE = 14f;

    // Page margins
    private const float MARGIN_TOP = 70f;
    private const float MARGIN_BOTTOM = 70f;
    private const float MARGIN_LEFT = 50f;
    private const float MARGIN_RIGHT = 50f;

    public Cookbook(ILogger<Cookbook> logger, IRecipeService recipeService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));
    }

    /// <summary>
    /// Creates a recipe cookbook PDF file with the specified parameters
    /// </summary>
    /// <param name="outputPath">Path where the PDF will be saved</param>
    /// <param name="name">Title of the cookbook</param>
    /// <param name="description">Description of the cookbook</param>
    /// <returns>The path to the created PDF file, or null if an error occurred</returns>
    public string? MakeCookbook(string outputPath, string name, string description)
    {
        _logger.LogInformation("Starting to generate cookbook PDF: {Name}", name);

        try
        {
            // Create PDF document with writer
            PdfWriter writer = new(outputPath);
            PdfDocument pdfDoc = new(writer);

            // Create Document with margins
            Document doc = new(pdfDoc, PageSize.A4, false);
            doc.SetMargins(MARGIN_TOP, MARGIN_RIGHT, MARGIN_BOTTOM, MARGIN_LEFT);

            // Create the cookbook content
            CreateCookbook(doc, pdfDoc, name, description);

            // Close the document
            doc.Close();
            pdfDoc.Close();

            _logger.LogInformation("Successfully generated cookbook PDF: {OutputPath}", outputPath);
            return outputPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating cookbook PDF: {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Creates the complete cookbook structure
    /// </summary>
    private void CreateCookbook(Document doc, PdfDocument pdfDoc, string name, string description)
    {
        // Load all recipes
        List<RecipeModel> recipes = _recipeService.Get().ToList();
        if (recipes.Count == 0)
        {
            _logger.LogWarning("No recipes found to include in the cookbook");
            doc.Add(new Paragraph("No recipes available.")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFont(PdfFontFactory.CreateFont(PRIMARY_FONT))
                .SetFontSize(BODY_FONT_SIZE));
            return;
        }

        // Load styles
        StyleDictionary styles = CreateStyles();

        // Track TOC entries
        List<TocEntry> tocEntries = [];

        // Add title page
        AddTitlePage(doc, name, description, styles);

        // Reserve page for TOC
        doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
        int tocPageNumber = pdfDoc.GetNumberOfPages();

        // Add recipes grouped by category
        AddRecipesByCategory(doc, recipes, tocEntries, styles);

        // Go back and add TOC at the reserved page
        AddTableOfContents(pdfDoc, tocPageNumber, tocEntries, styles);

        // Add recipe index at the end
        AddRecipeIndex(doc, recipes, styles);
    }

    /// <summary>
    /// Creates a dictionary of styles used throughout the document
    /// </summary>
    private StyleDictionary CreateStyles()
    {
        return new StyleDictionary
        {
            Title = new Style()
                .SetFont(PdfFontFactory.CreateFont(HEADING_FONT))
                .SetFontSize(TITLE_FONT_SIZE)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(ColorConstants.BLACK),

            Description = new Style()
                .SetFont(PdfFontFactory.CreateFont(PRIMARY_FONT))
                .SetFontSize(BODY_FONT_SIZE + 6)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(ColorConstants.DARK_GRAY)
                .SetMarginTop(20),

            CategoryTitle = new Style()
                .SetFont(PdfFontFactory.CreateFont(HEADING_FONT))
                .SetFontSize(CATEGORY_FONT_SIZE)
                .SetFontColor(ColorConstants.BLUE)
                .SetMarginTop(30)
                .SetMarginBottom(20),

            RecipeTitle = new Style()
                .SetFont(PdfFontFactory.CreateFont(HEADING_FONT))
                .SetFontSize(RECIPE_TITLE_FONT_SIZE)
                .SetFontColor(ColorConstants.DARK_GRAY)
                .SetMarginTop(20)
                .SetMarginBottom(10),

            SectionTitle = new Style()
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(SECTION_TITLE_FONT_SIZE)
                .SetMarginTop(15)
                .SetMarginBottom(5),

            Body = new Style()
                .SetFont(PdfFontFactory.CreateFont(PRIMARY_FONT))
                .SetFontSize(BODY_FONT_SIZE)
                .SetFontColor(ColorConstants.BLACK),

            TocTitle = new Style()
                .SetFont(PdfFontFactory.CreateFont(HEADING_FONT))
                .SetFontSize(RECIPE_TITLE_FONT_SIZE)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(ColorConstants.BLACK)
                .SetMarginBottom(20),

            TocEntry = new Style()
                .SetFont(PdfFontFactory.CreateFont(PRIMARY_FONT))
                .SetFontSize(BODY_FONT_SIZE)
                .SetFontColor(ColorConstants.BLACK)
                .SetMarginBottom(5)
        };
    }

    /// <summary>
    /// Adds the title page to the document
    /// </summary>
    private void AddTitlePage(Document doc, string name, string description, StyleDictionary styles)
    {
        // Add some space at the top
        doc.Add(new Paragraph("\n\n\n"));

        // Add title
        doc.Add(new Paragraph(name).AddStyle(styles.Title));

        // Add description with some space
        doc.Add(new Paragraph("\n"));
        doc.Add(new Paragraph(description).AddStyle(styles.Description));

        // Add date
        doc.Add(new Paragraph("\n\n\nCreated: " + DateTime.Now.ToString("MMMM d, yyyy"))
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFont(PdfFontFactory.CreateFont(PRIMARY_FONT))
            .SetFontSize(BODY_FONT_SIZE)
            .SetFontColor(ColorConstants.GRAY));

        // Add page break
        doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
    }

    /// <summary>
    /// Adds the table of contents at the specified page
    /// </summary>
    private void AddTableOfContents(PdfDocument pdfDoc, int tocPageNumber, List<TocEntry> tocEntries, StyleDictionary styles)
    {
        // Create a new content for the TOC page
        PdfPage tocPage = pdfDoc.GetPage(tocPageNumber);
        Rectangle pageSize = tocPage.GetPageSize();
        PdfCanvas canvas = new(tocPage.NewContentStreamBefore(), tocPage.GetResources(), pdfDoc);

        // Create a document for this specific page
        Document tocDoc = new(pdfDoc, new PageSize(pageSize), false);
        tocDoc.SetRenderer(new CanvasRenderer(tocDoc, canvas));

        // Add TOC title
        tocDoc.Add(new Paragraph("Table of Contents").AddStyle(styles.TocTitle));

        // Add TOC entries
        foreach (var entry in tocEntries)
        {
            Paragraph tocEntry = new Paragraph(entry.Title).AddStyle(styles.TocEntry);
            // Use DashedLine for tab leaders
            tocEntry.AddTabStops(new TabStop(450, TabAlignment.RIGHT, new DashedLine()));
            tocEntry.Add(new Tab());
            tocEntry.Add(entry.PageNumber.ToString());

            tocDoc.Add(tocEntry);
        }

        tocDoc.Flush();
    }

    /// <summary>
    /// Adds all recipes grouped by category to the document
    /// </summary>
    private void AddRecipesByCategory(Document doc, List<RecipeModel> recipes, List<TocEntry> tocEntries, StyleDictionary styles)
    {
        // Group recipes by category
        var recipesByCategory = recipes
            .GroupBy(r => r.RecipeCategory.Name)
            .OrderBy(g => g.Key)
            .ToList();

        foreach (var categoryGroup in recipesByCategory)
        {
            string categoryName = categoryGroup.Key ?? "Uncategorized";

            // Add category title
            doc.Add(new Paragraph(categoryName).AddStyle(styles.CategoryTitle));

            // Add to TOC
            int categoryPageNum = doc.GetPdfDocument().GetNumberOfPages();
            tocEntries.Add(new TocEntry(categoryName, categoryPageNum));

            // Add each recipe in the category
            foreach (var recipe in categoryGroup.OrderBy(r => r.Name))
            {
                AddRecipe(doc, recipe, styles);
            }

            // Add page break after category
            doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
        }
    }

    /// <summary>
    /// Adds a single recipe to the document
    /// </summary>
    private void AddRecipe(Document doc, RecipeModel recipe, StyleDictionary styles)
    {
        // Add page break before each recipe
        doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

        // Add recipe title
        doc.Add(new Paragraph(recipe.Name).AddStyle(styles.RecipeTitle));

        // Add recipe source if available
        if (!string.IsNullOrWhiteSpace(recipe.AuthorNM))
        {
            Paragraph authorPara = new Paragraph("By " + recipe.AuthorNM).AddStyle(styles.Body);
            authorPara.SetFontColor(ColorConstants.GRAY);
            doc.Add(authorPara);
        }

        // Add recipe description if available
        if (!string.IsNullOrWhiteSpace(recipe.Description))
        {
            doc.Add(new Paragraph(recipe.Description).AddStyle(styles.Body));
        }

        // Add servings info
        if (recipe.Servings > 0)
        {
            doc.Add(new Paragraph("Servings: " + recipe.Servings).AddStyle(styles.Body));
        }

        // Add divider
        doc.Add(new LineSeparator(new SolidLine(1f))
            .SetMarginTop(10)
            .SetMarginBottom(10));

        // Add ingredients with formatting
        doc.Add(new Paragraph("Ingredients").AddStyle(styles.SectionTitle));
        AddFormattedIngredients(doc, recipe.Ingredients, styles);

        // Add instructions with formatting  
        doc.Add(new Paragraph("Instructions").AddStyle(styles.SectionTitle));
        AddFormattedInstructions(doc, recipe.Instructions, styles);

        // Add bottom margin space
        doc.Add(new Paragraph("\n"));
    }

    /// <summary>
    /// Formats and adds the ingredients list from text
    /// </summary>
    private void AddFormattedIngredients(Document doc, string ingredients, StyleDictionary styles)
    {
        if (string.IsNullOrWhiteSpace(ingredients))
        {
            doc.Add(new Paragraph("No ingredients listed.").AddStyle(styles.Body));
            return;
        }

        // Create a list for ingredients
        List list = new List()
            .SetSymbolIndent(12)
            .SetListSymbol("•")
            .SetFont(PdfFontFactory.CreateFont(PRIMARY_FONT))
            .SetFontSize(BODY_FONT_SIZE);

        // Split ingredient lines
        string[] lines = ingredients.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (!string.IsNullOrWhiteSpace(trimmedLine))
            {
                list.Add(new ListItem(trimmedLine));
            }
        }

        doc.Add(list);
    }

    /// <summary>
    /// Formats and adds the instructions from text
    /// </summary>
    private void AddFormattedInstructions(Document doc, string instructions, StyleDictionary styles)
    {
        if (string.IsNullOrWhiteSpace(instructions))
        {
            doc.Add(new Paragraph("No instructions provided.").AddStyle(styles.Body));
            return;
        }

        // Split by lines or numbered steps
        string[] steps = Regex.Split(instructions, @"(?:\r\n|\r|\n)|(?<=\d\.)\s+")
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .ToArray();

        // Create numbered list if there aren't already numbers
        if (!Regex.IsMatch(steps[0], @"^\d+\."))
        {
            List list = new List(ListNumberingType.DECIMAL)
                .SetFont(PdfFontFactory.CreateFont(PRIMARY_FONT))
                .SetFontSize(BODY_FONT_SIZE);

            foreach (string step in steps)
            {
                list.Add(new ListItem(step));
            }

            doc.Add(list);
        }
        else
        {
            // Already numbered, add as paragraphs
            foreach (string step in steps)
            {
                doc.Add(new Paragraph(step).AddStyle(styles.Body));
            }
        }
    }

    /// <summary>
    /// Adds an index of all recipes at the end
    /// </summary>
    private void AddRecipeIndex(Document doc, List<RecipeModel> recipes, StyleDictionary styles)
    {
        doc.Add(new Paragraph("Recipe Index").AddStyle(styles.CategoryTitle));

        // Create alphabetically sorted list
        var sortedRecipes = recipes
            .OrderBy(r => r.Name)
            .ToList();

        // Create multi-column layout
        float[] columnWidths = { 1, 1 };
        Table table = new Table(UnitValue.CreatePercentArray(columnWidths))
            .SetWidth(UnitValue.CreatePercentValue(100))
            .SetBorder(Border.NO_BORDER);

        // Distribute recipes between columns
        int halfCount = (sortedRecipes.Count + 1) / 2;
        for (int i = 0; i < halfCount; i++)
        {
            Cell leftCell = new Cell().SetBorder(Border.NO_BORDER);
            leftCell.Add(new Paragraph(sortedRecipes[i].Name).AddStyle(styles.Body));
            table.AddCell(leftCell);

            if (i + halfCount < sortedRecipes.Count)
            {
                Cell rightCell = new Cell().SetBorder(Border.NO_BORDER);
                rightCell.Add(new Paragraph(sortedRecipes[i + halfCount].Name).AddStyle(styles.Body));
                table.AddCell(rightCell);
            }
            else
            {
                table.AddCell(new Cell().SetBorder(Border.NO_BORDER));
            }
        }

        doc.Add(table);
    }

    /// <summary>
    /// Class to store TOC entries with title and page number
    /// </summary>
    private class TocEntry
    {
        public string Title { get; }
        public int PageNumber { get; }

        public TocEntry(string title, int pageNumber)
        {
            Title = title;
            PageNumber = pageNumber;
        }
    }

    /// <summary>
    /// Class to hold all document styles
    /// </summary>
    private class StyleDictionary
    {
        public Style Title { get; set; }
        public Style Description { get; set; }
        public Style CategoryTitle { get; set; }
        public Style RecipeTitle { get; set; }
        public Style SectionTitle { get; set; }
        public Style Body { get; set; }
        public Style TocTitle { get; set; }
        public Style TocEntry { get; set; }
    }

    /// <summary>
    /// Canvas renderer for TOC placement
    /// </summary>
    private class CanvasRenderer : DocumentRenderer
    {
        private readonly PdfCanvas _canvas;

        public CanvasRenderer(Document document, PdfCanvas canvas) : base(document)
        {
            _canvas = canvas;
        }

        public override void Close()
        {
            base.Close();
            _canvas.Release();
        }
    }
}