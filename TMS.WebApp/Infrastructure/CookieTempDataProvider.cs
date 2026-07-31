using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace TMS.WebApp.Infrastructure
{
    public class CookieTempDataProvider : ITempDataProvider
    {
        private const string CookieName = "tms_tempdata";

        public IDictionary<string, object> LoadTempData(ControllerContext controllerContext)
        {
            var cookie = controllerContext.HttpContext.Request.Cookies[CookieName];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
                return new Dictionary<string, object>();

            try
            {
                var decoded = HttpUtility.UrlDecode(cookie.Value);
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(decoded)
                       ?? new Dictionary<string, object>();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        public void SaveTempData(ControllerContext controllerContext, IDictionary<string, object> values)
        {
            var response = controllerContext.HttpContext.Response;
            if (response == null) return;

            var httpCookie = new HttpCookie(CookieName)
            {
                Path = "/",
                HttpOnly = true
            };

            if (values != null && values.Count > 0)
            {
                httpCookie.Value = HttpUtility.UrlEncode(JsonConvert.SerializeObject(values));
                httpCookie.Expires = DateTime.Now.AddMinutes(30);
            }
            else
            {
                httpCookie.Expires = DateTime.Now.AddDays(-1);
                httpCookie.Value = null;
            }

            response.Cookies.Add(httpCookie);
        }
    }
}
