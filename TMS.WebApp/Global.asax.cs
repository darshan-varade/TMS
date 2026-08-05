using System;
using System.Configuration;
using System.IO;
using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Serilog;
using Serilog.Sinks.MSSqlServer;

namespace TMS.WebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ConfigureLogging();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
        }

        private static void ConfigureLogging()
        {
            try
            {
                string logPath = @"D:\Logs\tms-.log";
                try
                {
                    Directory.CreateDirectory(@"D:\Logs");
                }
                catch
                {
                    string fallbackDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Logs");
                    Directory.CreateDirectory(fallbackDir);
                    logPath = Path.Combine(fallbackDir, "tms-.log");
                }

                var fileConfig = new LoggerConfiguration()
                    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day);

                try
                {
                    Log.Logger = fileConfig
                        .WriteTo.MSSqlServer(
                            ConfigurationManager.ConnectionStrings["constr"].ConnectionString,
                            sinkOptions: new MSSqlServerSinkOptions { TableName = "tmsLogEvents", AutoCreateSqlTable = true })
                        .CreateLogger();
                }
                catch
                {
                    Log.Logger = fileConfig.CreateLogger();
                }
            }
            catch
            {
                Log.Logger = new LoggerConfiguration().CreateLogger();
            }
        }

        protected void Application_End()
        {
            Log.CloseAndFlush();
        }

        protected void Application_Error()
        {
            Exception ex = Server.GetLastError();
            if (ex == null) return;

            var httpEx = ex as HttpException;
            if (httpEx != null && httpEx.GetHttpCode() == 404)
            {
                Server.ClearError();
                Response.Redirect("~/Error/NotFound");
                return;
            }

            Log.Error(ex, "Unhandled application error");

            Server.ClearError();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                (Request.Headers["Accept"]?.Contains("application/json") ?? false))
            {
                Response.Clear();
                Response.StatusCode = 200;
                Response.ContentType = "application/json";
                Response.Write(new System.Web.Script.Serialization.JavaScriptSerializer()
                    .Serialize(new { success = false, message = "Something went wrong. Please try again." }));
                Response.End();
            }
            else
            {
                Response.Redirect("~/Error");
            }
        }
    }
}
