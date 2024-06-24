using Microsoft.AspNetCore.Mvc;

namespace WebSpark.Admin.Areas.Prompt.Controllers;


/// <summary>
/// 
/// </summary>
public class MainController : Controllers.PromptBaseController
{
    public IActionResult Index()
    {
        return View();
    }
}
