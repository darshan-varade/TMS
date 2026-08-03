using System.Web.Mvc;

namespace TMS.WebApp.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            Response.StatusCode = 500;
            return View("Error");
        }

        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View("NotFound");
        }
    }
}
