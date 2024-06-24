using Microsoft.AspNetCore.Mvc;

namespace AspnetCoreMvcFull.Controllers;

public class FormLayoutsController : Controller
{
  public IActionResult Horizontal() => View();
  public IActionResult Vertical() => View();
}
