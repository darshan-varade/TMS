using System.Web.Mvc;

namespace TMS.WebApp.Controllers
{
    [AuthorizeRole(Role.Administrator)]
    public class AdminController : BaseController
    {
        public ActionResult Index()
        {
            return RedirectToAction("Index", "User");
        }
    }
}
