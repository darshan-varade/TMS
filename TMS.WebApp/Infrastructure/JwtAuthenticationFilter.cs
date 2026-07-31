using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using TMS.DataAccess.DAL;

namespace TMS.WebApp.Infrastructure
{
    public class JwtAuthenticationFilter : IAuthenticationFilter
    {
        public void OnAuthentication(AuthenticationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var response = filterContext.HttpContext.Response;

            string accessToken = request.Cookies["access_token"]?.Value;

            if (!string.IsNullOrEmpty(accessToken))
            {
                var principal = JwtHelper.ValidateAccessToken(accessToken);
                if (principal != null)
                {
                    filterContext.HttpContext.User = principal;
                    return;
                }
            }

            string refreshToken = request.Cookies["refresh_token"]?.Value;
            if (!string.IsNullOrEmpty(refreshToken))
            {
                try
                {
                    var dal = new AuthDAL();
                    var result = JwtHelper.RefreshAccessToken(refreshToken, dal);

                    if (result?.Principal != null)
                    {
                        filterContext.HttpContext.User = result.Principal;

                        bool rememberMe = false;
                        var expiryCookie = request.Cookies["access_token"]?.Expires;
                        if (expiryCookie.HasValue && expiryCookie.Value > DateTime.Now.AddHours(1))
                            rememberMe = true;

                        DateTime accessExpiry = JwtHelper.GetAccessTokenExpiry(rememberMe);

                        response.Cookies.Add(new HttpCookie("access_token", result.AccessToken)
                        {
                            HttpOnly = true,
                            Secure = false,
                            Path = "/",
                            Expires = accessExpiry
                        });

                        response.Cookies.Add(new HttpCookie("refresh_token", result.RefreshToken)
                        {
                            HttpOnly = true,
                            Secure = false,
                            Path = "/",
                            Expires = result.RefreshExpiry
                        });

                        return;
                    }
                }
                catch
                {
                }
            }

            response.Cookies.Add(new HttpCookie("access_token", "") { Expires = DateTime.Now.AddDays(-1), Path = "/" });
            response.Cookies.Add(new HttpCookie("refresh_token", "") { Expires = DateTime.Now.AddDays(-1), Path = "/" });
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            var result = filterContext.Result as HttpUnauthorizedResult;
            if (result != null)
            {
                if (IsAjaxRequest(filterContext.HttpContext.Request))
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new { success = false, message = "Session expired. Please login again." },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    filterContext.Result = new RedirectResult("~/Auth/Login");
                }
            }
        }

        private bool IsAjaxRequest(HttpRequestBase request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   request.Headers["Accept"]?.Contains("application/json") == true;
        }
    }
}
