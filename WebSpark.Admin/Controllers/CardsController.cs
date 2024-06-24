using Microsoft.AspNetCore.Mvc;

namespace AspnetCoreMvcFull.Controllers;

public class CardsController : Controller
{
  public IActionResult Basic() => View();
}
