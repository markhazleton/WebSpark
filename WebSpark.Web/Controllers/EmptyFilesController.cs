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
        logger.LogInformation("Serving blank JS file");
        return BlankFile("application/javascript");
    }

    /// <summary>
    /// Action Method for service blank PHP file 
    /// </summary>
    /// <returns></returns>
    [Route("/blank.php")]
    public ContentResult BlankPHP()
    {
        return BlankFile("application/x-httpd-php");
    }

    /// <summary>
    /// Action method for serving blank CSS file.
    /// </summary>
    /// <returns>The content result.</returns>
    [Route("/blank.css")]
    public ContentResult BlankCSS()
    {
        logger.LogInformation("Serving blank CSS file");
        return BlankFile("text/css");
    }

    /// <summary>
    /// Action method for serving blank PNG file.
    /// </summary>
    /// <returns>The file result.</returns>
    [Route("/blank.png")]
    public IActionResult BlankPNG()
    {
        logger.LogInformation("Serving blank PNG file");
        return BlankImage("image/png");
    }

    /// <summary>
    /// Action method for serving blank JPG file.
    /// </summary>
    /// <returns>The file result.</returns>
    [Route("/blank.jpg")]
    public IActionResult BlankJPG()
    {
        logger.LogInformation("Serving blank JPG file");
        return BlankImage("image/jpeg");
    }

    /// <summary>
    /// Action method for serving blank GIF file.
    /// </summary>
    /// <returns>The file result.</returns>
    [Route("/blank.gif")]
    public IActionResult BlankGIF()
    {
        logger.LogInformation("Serving blank GIF file");
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
