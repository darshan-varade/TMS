using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using TMS.DataAccess.DAL;
using TMS.DataAccess.ViewModels;
using TMS.WebApp.Infrastructure;

namespace TMS.WebApp.Controllers
{
    [AllowAnonymous]
    public class AuthController : BaseController
    {
        [HttpGet]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                AuthDAL dal = new AuthDAL();
                var user = dal.UserLogin(vm.Email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(vm.Password, user.PasswordHash))
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                    return View(vm);
                }

                if (user.IsApproved == null)
                {
                    ModelState.AddModelError("", "Your account is awaiting admin approval. Please contact admin.");
                    return View(vm);
                }

                if (user.IsApproved.Value == 0)
                {
                    ModelState.AddModelError("", "Your account has been rejected. Contact admin.");
                    return View(vm);
                }

                dal.UpdateLastLogin(user.UserId);

                string accessToken = JwtHelper.GenerateAccessToken(user.UserId, user.FullName, user.EmailId, user.RoleName, user.MobileNumber, user.DepartmentName);
                string refreshToken = JwtHelper.GenerateRefreshToken();
                string refreshTokenHash = JwtHelper.HashRefreshToken(refreshToken);
                DateTime refreshExpiry = DateTime.Now.AddDays(int.Parse(ConfigurationManager.AppSettings["JwtRefreshTokenExpiryDays"] ?? "7"));

                dal.CreateRefreshToken(user.UserId, refreshTokenHash, refreshExpiry);

                DateTime accessExpiry = JwtHelper.GetAccessTokenExpiry(vm.RememberMe);

                Response.Cookies.Add(new HttpCookie("access_token", accessToken)
                {
                    HttpOnly = true,
                    Secure = false,
                    Path = "/",
                    Expires = accessExpiry
                });

                Response.Cookies.Add(new HttpCookie("refresh_token", refreshToken)
                {
                    HttpOnly = true,
                    Secure = false,
                    Path = "/",
                    Expires = refreshExpiry
                });

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error in Login POST");
                ModelState.AddModelError("", "An error occurred. Please try again.");
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult Signup()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            MasterDataDAL dal = new MasterDataDAL();
            var vm = new SignupViewModel();
            ViewBag.Departments = new SelectList(dal.GetDepartments(), "Id", "Name");
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Signup(SignupViewModel vm)
        {
            AuthDAL dal = new AuthDAL();

            if (vm.SignupStep == "otp")
            {
                ModelState.Remove("FullName");
                ModelState.Remove("MobileNumber");
                ModelState.Remove("Email");
                ModelState.Remove("Password");
                ModelState.Remove("ConfirmPassword");

                if (string.IsNullOrEmpty(vm.OtpCode) || vm.OtpCode.Length != 6)
                {
                    ModelState.AddModelError("OtpCode", "Enter the 6-digit code.");
                    ViewBag.Departments = new SelectList(new MasterDataDAL().GetDepartments(), "Id", "Name");
                    return View(vm);
                }

                try
                {
                    int? otpId = dal.ValidateOtpByEmail(vm.OtpEmail, vm.OtpCode);
                    if (otpId == null)
                    {
                        ModelState.AddModelError("OtpCode", "Invalid or expired code.");
                        ViewBag.Departments = new SelectList(new MasterDataDAL().GetDepartments(), "Id", "Name");
                        return View(vm);
                    }

                    dal.MarkOtpUsed(otpId.Value);
                    int userId = dal.UserRegister(vm.FullName, vm.MobileNumber, vm.OtpEmail, vm.PasswordHash, vm.DepartmentId);

                    TempData["info"] = "Account created! Please wait for admin approval before you can login.";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error(ex, "Error in Signup OTP verification");
                    ModelState.AddModelError("", "An error occurred. Please try again.");
                    ViewBag.Departments = new SelectList(new MasterDataDAL().GetDepartments(), "Id", "Name");
                    return View(vm);
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Departments = new SelectList(new MasterDataDAL().GetDepartments(), "Id", "Name");
                return View(vm);
            }

            try
            {
                if (dal.UserCheckEmail(vm.Email))
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    ViewBag.Departments = new SelectList(new MasterDataDAL().GetDepartments(), "Id", "Name");
                    return View(vm);
                }

                string passwordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password);
                string otpCode = new Random().Next(100000, 999999).ToString();
                DateTime otpExpiresAt = DateTime.Now.AddMinutes(int.Parse(ConfigurationManager.AppSettings["OtpExpiryMinutes"] ?? "5"));

                dal.CreateOtpByEmail(vm.Email, otpCode, otpExpiresAt);
                EmailService.SendOtp(vm.Email, otpCode);

                vm.PasswordHash = passwordHash;
                vm.OtpEmail = vm.Email;
                vm.SignupStep = "otp";
                vm.Password = null;
                vm.ConfirmPassword = null;
                ModelState.Clear();
                ViewBag.Departments = new SelectList(new MasterDataDAL().GetDepartments(), "Id", "Name");

                return View(vm);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error in Signup POST");
                ModelState.AddModelError("", "An error occurred. Please try again.");
                ViewBag.Departments = new SelectList(new MasterDataDAL().GetDepartments(), "Id", "Name");
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ResendOtp(string email)
        {
            try
            {
                AuthDAL dal = new AuthDAL();

                DateTime? lastOtp = dal.GetLatestOtpTimeByEmail(email);
                if (lastOtp.HasValue && (DateTime.Now - lastOtp.Value).TotalSeconds < 60)
                {
                    return Json(new { success = false, error = "Please wait 60 seconds before requesting a new code." });
                }

                string otpCode = new Random().Next(100000, 999999).ToString();
                DateTime otpExpiresAt = DateTime.Now.AddMinutes(int.Parse(ConfigurationManager.AppSettings["OtpExpiryMinutes"] ?? "5"));

                dal.CreateOtpByEmail(email, otpCode, otpExpiresAt);
                EmailService.SendOtp(email, otpCode);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error in ResendOtp");
                return Json(new { success = false, error = "An error occurred. Please try again." });
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            string refreshToken = Request.Cookies["refresh_token"]?.Value;
            if (!string.IsNullOrEmpty(refreshToken))
            {
                string hash = JwtHelper.HashRefreshToken(refreshToken);
                var record = new AuthDAL().GetRefreshTokenByHash(hash);
                if (record != null)
                    new AuthDAL().RevokeRefreshToken(record.RefreshTokenId);
            }

            Response.Cookies.Add(new HttpCookie("access_token", "") { Expires = DateTime.Now.AddDays(-1), Path = "/" });
            Response.Cookies.Add(new HttpCookie("refresh_token", "") { Expires = DateTime.Now.AddDays(-1), Path = "/" });

            return RedirectToAction("Login");
        }
    }
}
