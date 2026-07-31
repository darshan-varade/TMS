using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace TMS.WebApp.Infrastructure
{
    public class CookieTempDataProvider : ITempDataProvider
    {
        private const string CookieName = "tms_tempdata";
        private static readonly byte[] SigningKey = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["TempDataKey"] ?? "tms-tempdata-key");

        public IDictionary<string, object> LoadTempData(ControllerContext controllerContext)
        {
            var cookie = controllerContext.HttpContext.Request.Cookies[CookieName];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
                return new Dictionary<string, object>();

            try
            {
                string payload;
                if (!TryVerify(cookie.Value, out payload))
                    return new Dictionary<string, object>();

                var decoded = HttpUtility.UrlDecode(payload);
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
                string payload = HttpUtility.UrlEncode(JsonConvert.SerializeObject(values));
                httpCookie.Value = Sign(payload);
                httpCookie.Expires = DateTime.Now.AddMinutes(30);
            }
            else
            {
                httpCookie.Expires = DateTime.Now.AddDays(-1);
                httpCookie.Value = null;
            }

            response.Cookies.Add(httpCookie);
        }

        private static string Sign(string value)
        {
            using (var hmac = new HMACSHA256(SigningKey))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(value));
                return value + "." + Convert.ToBase64String(hash);
            }
        }

        private static bool TryVerify(string signed, out string value)
        {
            value = null;
            if (string.IsNullOrEmpty(signed)) return false;

            int idx = signed.LastIndexOf('.');
            if (idx <= 0 || idx == signed.Length - 1) return false;

            string payload = signed.Substring(0, idx);
            string provided = signed.Substring(idx + 1);

            byte[] expectedHash;
            using (var hmac = new HMACSHA256(SigningKey))
            {
                expectedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            }

            string expected = Convert.ToBase64String(expectedHash);
            if (!CryptographicEquals(provided, expected)) return false;

            value = payload;
            return true;
        }

        private static bool CryptographicEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
