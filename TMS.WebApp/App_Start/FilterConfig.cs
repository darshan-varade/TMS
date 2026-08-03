using System.Web;
using System.Web.Mvc;
using TMS.WebApp.Infrastructure;

namespace TMS.WebApp
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleExceptionAttribute());
            filters.Add(new JwtAuthenticationFilter());
            filters.Add(new AuthorizeAttribute());
        }
    }
}
