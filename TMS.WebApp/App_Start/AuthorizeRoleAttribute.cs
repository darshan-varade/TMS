using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace TMS.WebApp
{
    public enum Role
    {
        Administrator,
        SupportExecutive,
        Employee
    }

    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        private readonly Role[] _allowedRoles;

        public AuthorizeRoleAttribute(params Role[] roles)
        {
            _allowedRoles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext)) return false;
            foreach (Role role in _allowedRoles)
            {
                if (httpContext.User.IsInRole(GetDbRoleName(role)))
                    return true;
            }
            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new JsonResult
                {
                    Data = new { success = false, message = "Unauthorized" },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary { { "controller", "Home" }, { "action", "Index" } });
            }
        }

        private static string GetDbRoleName(Role role)
        {
            switch (role)
            {
                case Role.Administrator: return "Administrator";
                case Role.SupportExecutive: return "Support Executive";
                case Role.Employee: return "Employee";
                default: return role.ToString();
            }
        }
    }
}
