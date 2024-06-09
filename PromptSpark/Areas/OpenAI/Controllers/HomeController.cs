namespace PromptSpark.Areas.OpenAI.Controllers;
[Area("OpenAI")]
public class HomeController : OpenAIBaseController
{
    public IActionResult Index()
    {
        return View();
    }
}
