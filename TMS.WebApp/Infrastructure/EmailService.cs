using System;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Web.Hosting;
using Serilog;

namespace TMS.WebApp.Infrastructure
{
    public static class EmailService
    {
        public static void SendOtp(string toEmail, string otpCode)
        {
            try
            {
                using (SmtpClient client = new SmtpClient())
                using (MailMessage msg = new MailMessage())
                {
                    msg.To.Add(toEmail);
                    msg.Subject = ConfigurationManager.AppSettings["OtpEmailSubject"] ?? "Your OTP Code - TMS";
                    msg.IsBodyHtml = true;

                    string templateFolder = ConfigurationManager.AppSettings["EmailTemplatePath"] ?? "~/EmailTemplates";
                    string templatePath = templateFolder.StartsWith("~/")
                        ? HostingEnvironment.MapPath(templateFolder + "/OtpEmail.html")
                        : Path.Combine(templateFolder, "OtpEmail.html");
                    msg.Body = File.ReadAllText(templatePath).Replace("{otpCode}", otpCode);

                    client.Send(msg);
                    Log.Information("OTP email sent to {Email}", toEmail);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send OTP email to {Email}", toEmail);
                throw;
            }

            Log.Information("OTP for {Email}: {OtpCode}", toEmail, otpCode);
        }
    }
}
