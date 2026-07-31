using System;
using System.Web.Mvc;
using TMS.DataAccess.DAL;
using TMS.DataAccess.ViewModels;

namespace TMS.WebApp.Controllers
{
    public class ProfileController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.Title = "My Profile";
            UserDAL dal = new UserDAL();
            var user = dal.GetUserById(CurrentUserId);
            if (user == null)
                return RedirectToAction("Logout", "Auth");

            var vm = new ProfileViewModel
            {
                FullName = user.FullName,
                MobileNumber = user.MobileNumber,
                Email = user.EmailId,
                DepartmentName = user.DepartmentName,
                RoleName = user.RoleName
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(ProfileViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                UserDAL dal = new UserDAL();
                var user = dal.GetUserById(CurrentUserId);
                vm.Email = user?.EmailId;
                vm.DepartmentName = user?.DepartmentName;
                vm.RoleName = user?.RoleName;
                return View("Index", vm);
            }

            try
            {
                UserDAL dal = new UserDAL();
                dal.UpdateProfile(CurrentUserId, vm.FullName, vm.MobileNumber);
                TempData["info"] = "Profile updated successfully.";
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error updating profile");
                TempData["info"] = "Error updating profile.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["info"] = "Please fix the errors.";
                return RedirectToAction("Index");
            }

            try
            {
                UserDAL dal = new UserDAL();
                var user = dal.GetUserById(CurrentUserId);

                if (!BCrypt.Net.BCrypt.Verify(vm.CurrentPassword, user.PasswordHash))
                {
                    TempData["info"] = "Current password is incorrect.";
                    return RedirectToAction("Index");
                }

                string newHash = BCrypt.Net.BCrypt.HashPassword(vm.NewPassword);
                dal.ChangePassword(user.CredentialId, newHash);
                TempData["info"] = "Password changed successfully.";
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error changing password");
                TempData["info"] = "Error changing password.";
            }

            return RedirectToAction("Index");
        }
    }
}
