using Microsoft.AspNetCore.Mvc;

namespace AspnetCoreMvcFull.Controllers;

public class FormsController : Controller
{
  public IActionResult BasicInputs() => View();
  public IActionResult InputGroups() => View();
}
