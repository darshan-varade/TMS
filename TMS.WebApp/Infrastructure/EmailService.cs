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

                    string templatePath = HostingEnvironment.MapPath("~/EmailTemplates/OtpEmail.html");
                    if (File.Exists(templatePath))
                        msg.Body = File.ReadAllText(templatePath).Replace("{otpCode}", otpCode);
                    else
                        msg.Body = "<h2>Your OTP Code</h2><p>Your OTP is: <strong>" + otpCode + "</strong></p>";

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
