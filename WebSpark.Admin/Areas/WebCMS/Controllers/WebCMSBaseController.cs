using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebSpark.Domain.ViewModels;

namespace WebSpark.Admin.Areas.WebCMS.Controllers;

[Area("WebCMS")]
public class WebCMSBaseController() : Controller
{
  [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
  public IActionResult Error()
  {
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
  }
}
