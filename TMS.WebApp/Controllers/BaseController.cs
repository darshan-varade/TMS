using System.Security.Claims;
using System.Web.Mvc;
using TMS.WebApp.Infrastructure;

namespace TMS.WebApp.Controllers
{
    public class BaseController : Controller
    {
        public BaseController()
        {
            TempDataProvider = new CookieTempDataProvider();
        }

        protected int CurrentUserId
        {
            get
            {
                var claim = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.NameIdentifier);
                return claim != null ? int.Parse(claim.Value) : 0;
            }
        }

        protected string CurrentRoleName
        {
            get
            {
                var claim = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.Role);
                return claim?.Value;
            }
        }

        protected bool IsAdmin => User.IsInRole("Administrator");
        protected bool IsSupport => User.IsInRole("Support Executive");
        protected bool IsEmployee => User.IsInRole("Employee");

        protected string CurrentUserName
        {
            get
            {
                var claim = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.Name);
                return claim?.Value;
            }
        }

        protected string CurrentUserEmail
        {
            get
            {
                var claim = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.Email);
                return claim?.Value;
            }
        }

        protected string CurrentUserMobile
        {
            get
            {
                var claim = ((ClaimsPrincipal)User).FindFirst("Mobile");
                return claim?.Value;
            }
        }

        protected string CurrentUserDepartment
        {
            get
            {
                var claim = ((ClaimsPrincipal)User).FindFirst("Department");
                return claim?.Value;
            }
        }

        protected ActionResult RequireAdmin()
        {
            if (!IsAdmin) return RedirectToAction("Index", "Home");
            return null;
        }

        protected string GetNormalizedRoleName()
        {
            string role = CurrentRoleName;
            if (role == "Administrator") return "Admin";
            if (role == "Support Executive") return "Support";
            return "Employee";
        }
    }
}
