namespace WebSpark.WebMvc.Areas.Prompt.Controllers;


/// <summary>
/// 
/// </summary>
public class MainController : OpenAIBaseController
{
    public IActionResult Index()
    {
        return View();
    }
}
