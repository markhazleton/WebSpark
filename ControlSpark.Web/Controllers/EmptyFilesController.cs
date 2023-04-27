namespace ControlSpark.Web.Controllers
{
    public class EmptyFilesController : Controller
    {
        [Route("/blank.js")]
        public ContentResult BlankJS()
        {
            return Content(string.Empty, "application/javascript");
        }

        [Route("/blank.css")]
        public ContentResult BlankCSS()
        {
            return Content(string.Empty, "text/css");
        }
    }
}
