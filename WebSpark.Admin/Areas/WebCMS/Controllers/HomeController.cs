using Microsoft.AspNetCore.Mvc;

namespace WebSpark.Admin.Areas.WebCMS.Controllers;
public class HomeController : WebCMSBaseController
{
  public IActionResult Index()
  {
    return View();
  }
}
