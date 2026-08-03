using System.Web;
using System.Web.Mvc;
using Serilog;

namespace TMS.WebApp.Infrastructure
{
    public class HandleExceptionAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null) throw new System.ArgumentNullException("filterContext");
            if (filterContext.ExceptionHandled) return;

            Log.Error(filterContext.Exception,
                "Unhandled exception in {Controller}.{Action}",
                filterContext.RouteData.Values["controller"],
                filterContext.RouteData.Values["action"]);

            if (IsAjaxRequest(filterContext.HttpContext.Request))
            {
                filterContext.Result = new JsonResult
                {
                    Data = new { success = false, message = "Something went wrong. Please try again." },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                filterContext.Result = new ViewResult { ViewName = "Error" };
            }

            filterContext.ExceptionHandled = true;
        }

        private static bool IsAjaxRequest(HttpRequestBase request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   request.Headers["Accept"]?.Contains("application/json") == true;
        }
    }
}
