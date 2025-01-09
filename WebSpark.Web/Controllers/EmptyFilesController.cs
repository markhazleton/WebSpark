namespace WebSpark.Web.Controllers;

/// <summary>
/// Controller for serving empty files.
/// </summary>
public class EmptyFilesController(IWebHostEnvironment env, ILogger<EmptyFilesController> logger) : Controller
{
    private readonly string _placeholderImagePath = Path.Combine(env.WebRootPath, "images", "placeholder.jpg");

    /// <summary>
    /// Action method for serving blank JS file.
    /// </summary>
    /// <returns>The content result.</returns>
    [Route("/blank.js")]
    public ContentResult BlankJS()
    {
        return BlankFile("application/javascript");
    }

    /// <summary>
    /// Action Method for service blank PHP file
    /// </summary>
    /// <returns></returns>
    [Route("/blank.php")]
    public ContentResult BlankPHP()
    {
        string htmlContent = @"
        <html>
        <head>
            <title>Redirecting...</title>
            <script src=""/_framework/aspnetcore-browser-refresh.js""></script>
            <script>
                // Redirect to the home page
                window.location.href = '/';
            </script>
        </head>
        <body>
            <!-- Nice Try! -->
        </body>
        </html>";

        return Content(htmlContent, "text/html");
    }

    /// <summary>
    /// Action method for serving blank CSS file.
    /// </summary>
    /// <returns>The content result.</returns>
    [Route("/blank.css")]
    public ContentResult BlankCSS()
    {
        return BlankFile("text/css");
    }

    /// <summary>
    /// Action method for serving blank PNG file.
    /// </summary>
    /// <returns>The file result.</returns>
    [Route("/blank.png")]
    public IActionResult BlankPNG()
    {
        return BlankImage("image/png");
    }

    /// <summary>
    /// Action method for serving blank JPG file.
    /// </summary>
    /// <returns>The file result.</returns>
    [Route("/blank.jpg")]
    public IActionResult BlankJPG()
    {
        return BlankImage("image/jpeg");
    }

    /// <summary>
    /// Action method for serving blank GIF file.
    /// </summary>
    /// <returns>The file result.</returns>
    [Route("/blank.gif")]
    public IActionResult BlankGIF()
    {
        return BlankImage("image/gif");
    }

    /// <summary>
    /// Returns a blank content result.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>The content result.</returns>
    private ContentResult BlankFile(string contentType)
    {
        return Content(string.Empty, contentType);
    }

    /// <summary>
    /// Returns a blank image file result.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>The file result.</returns>
    private FileContentResult BlankImage(string contentType)
    {
        var imageBytes = System.IO.File.ReadAllBytes(_placeholderImagePath);
        return File(imageBytes, contentType);
    }
}
