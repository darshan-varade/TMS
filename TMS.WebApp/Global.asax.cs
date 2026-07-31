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
                            sinkOptions: new MSSqlServerSinkOptions { TableName = "LogEvents", AutoCreateSqlTable = true })
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
    }
}
