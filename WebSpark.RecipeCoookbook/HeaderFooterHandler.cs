using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace WebSpark.RecipeCookbook;

/// <summary>
/// Separate class for adding headers and footers to PDF pages
/// Add this class to your project to enable PDF header and footer functionality
/// </summary>
public class HeaderFooterHandler
{
    private readonly string _title;

    public HeaderFooterHandler(string title)
    {
        _title = title;
    }

    /// <summary>
    /// Add header and footer to a PDF page
    /// </summary>
    /// <param name="pdfDoc">The PDF document</param>
    /// <param name="page">The page to add header/footer to</param>
    public void AddHeaderFooter(PdfDocument pdfDoc, PdfPage page)
    {
        int pageNumber = pdfDoc.GetPageNumber(page);

        // Skip header/footer on title page
        if (pageNumber == 1) return;

        Rectangle pageSize = page.GetPageSize();
        PdfCanvas canvas = new(page.NewContentStreamBefore(), page.GetResources(), pdfDoc);

        // Define positions
        float headerY = pageSize.GetTop() - 30;
        float footerY = pageSize.GetBottom() + 30;
        float centerX = pageSize.GetWidth() / 2;

        // Create canvas for adding content
        Canvas layoutCanvas = new(canvas, pageSize);

        // Add header text
        Paragraph headerText = new Paragraph(_title)
            .SetFontSize(8)
            .SetFontColor(ColorConstants.GRAY);

        headerText.SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE));
        layoutCanvas.ShowTextAligned(headerText, centerX, headerY, TextAlignment.CENTER);

        // Add footer with page number
        Paragraph footerText = new Paragraph($"Page {pageNumber}")
            .SetFontSize(8)
            .SetFontColor(ColorConstants.GRAY);
        layoutCanvas.ShowTextAligned(footerText, centerX, footerY, TextAlignment.CENTER);

        layoutCanvas.Close();
    }
}