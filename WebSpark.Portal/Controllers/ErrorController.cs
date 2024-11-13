namespace WebSpark.Portal.Controllers;

public class ErrorController(ILogger<ErrorController> _logger) : Controller
{
    // This action serves as the target for the 404 page
    [HttpGet]
    [Route("Error/404")]
    public IActionResult NotFoundPage()
    {
        LogError(404);

        return RedirectPermanent("/Error/NotFound");
    }

    // This action renders the 404 page after redirect
    [Route("Error/NotFound")]
    [HttpGet]
    public IActionResult RedirectTo404()
    {
        return View("NotFoundPage");
    }

    // Generic error handler for other status codes
    [HttpGet]
    [Route("Error/{statusCode}")]
    public IActionResult HandleErrorCode(int statusCode)
    {
        LogError(statusCode);

        if (statusCode == 404)
        {
            return RedirectToAction("NotFoundPage");
        }
        else if (statusCode == 500)
        {
            return View("ServerErrorPage");
        }
        else if (statusCode == 403)
        {
            return View("ForbiddenPage");
        }
        else
        {
            return View("GenericError");
        }
    }

    // Helper method to log the error with the requested URL
    private void LogError(int statusCode) => _logger.LogInformation("Error {StatusCode} - Requested Page: {RequestedUrl}", statusCode, HttpContext.Request.Path + HttpContext.Request.QueryString);
}