namespace ControlSpark.Web.Controllers
{
    public class EmptyFilesController : Controller
    {
        [Route("/blank.js")]
        public ContentResult BlankJS()
        {
            return Content("", "application/javascript");
        }

        [Route("/blank.css")]
        public ContentResult BlankCSS()
        {
            return Content("", "text/css");
        }
    }
}
