using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TMS.DataAccess.DAL;
using TMS.DataAccess.ViewModels;

namespace TMS.WebApp.Controllers
{
    [AuthorizeRole(Role.Administrator)]
    public class UserController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Users";
            MasterDataDAL master = new MasterDataDAL();
            ViewBag.Roles = new SelectList(master.GetRoles(), "Id", "Name");
            ViewBag.CurrentUserId = CurrentUserId;
            return View(new UserListViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public ActionResult IndexPost(UserListViewModel vm)
        {
            UserDAL dal = new UserDAL();

            int pageNumber = vm.PageNumber <= 0 ? 1 : vm.PageNumber;
            vm.PageSize = vm.PageSize <= 0 ? 10 : vm.PageSize;
            if (string.IsNullOrEmpty(vm.SortColumn)) vm.SortColumn = "CreatedOn";
            if (string.IsNullOrEmpty(vm.SortDirection)) vm.SortDirection = "DESC";

            int totalRows;
            vm.Users = dal.GetUserList(vm.SearchTerm, vm.RoleId, vm.SortColumn, vm.SortDirection, pageNumber, vm.PageSize, out totalRows);
            vm.TotalRows = totalRows;
            vm.PageNumber = pageNumber;

            ViewBag.Roles = new SelectList(new MasterDataDAL().GetRoles(), "Id", "Name", vm.RoleId);
            ViewBag.CurrentUserId = CurrentUserId;
            return PartialView("_UserListPartial", vm);
        }

        [HttpGet]
        public JsonResult UserSearch(string term)
        {
            try
            {
                int total;
                var users = new UserDAL().GetUserList(term ?? "", null, "CreatedOn", "ASC", 1, 20, out total);
                var results = users.Select(u => new
                {
                    id = u.UserId,
                    text = u.FullName
                });
                return Json(results, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error in UserSearch");
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Create()
        {
            ViewBag.Title = "Add User";
            MasterDataDAL master = new MasterDataDAL();
            var vm = new UserAddViewModel
            {
                Roles = master.GetRoles(),
                Departments = master.GetDepartments()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserAddViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                MasterDataDAL master = new MasterDataDAL();
                vm.Roles = master.GetRoles();
                vm.Departments = master.GetDepartments();
                return View(vm);
            }

            try
            {
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password);
                UserDAL dal = new UserDAL();
                dal.AddUser(vm.FullName, vm.MobileNumber, vm.Email, passwordHash, vm.RoleId, vm.DepartmentId, CurrentUserId);
                TempData["info"] = "User created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error creating user");
                ModelState.AddModelError("", "An error occurred.");
                MasterDataDAL master = new MasterDataDAL();
                vm.Roles = master.GetRoles();
                vm.Departments = master.GetDepartments();
                return View(vm);
            }
        }

        public ActionResult Edit(int id)
        {
            ViewBag.Title = "Edit User";
            UserDAL dal = new UserDAL();
            MasterDataDAL master = new MasterDataDAL();

            var user = dal.GetUserById(id);
            if (user == null)
            {
                TempData["info"] = "User not found.";
                return RedirectToAction("Index");
            }

            var vm = new UserEditViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                MobileNumber = user.MobileNumber,
                Email = user.EmailId,
                RoleId = user.RoleId,
                DepartmentId = user.DepartmentId,
                IsActive = user.IsActive,
                Roles = master.GetRoles(),
                Departments = master.GetDepartments()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                MasterDataDAL master = new MasterDataDAL();
                vm.Roles = master.GetRoles();
                vm.Departments = master.GetDepartments();
                return View(vm);
            }

            try
            {
                if (vm.UserId == CurrentUserId)
                {
                    UserDAL dal0 = new UserDAL();
                    var self = dal0.GetUserById(CurrentUserId);
                    if (self != null && self.RoleId != vm.RoleId)
                    {
                        ModelState.AddModelError("", "You cannot change your own role.");
                        MasterDataDAL master0 = new MasterDataDAL();
                        vm.Roles = master0.GetRoles();
                        vm.Departments = master0.GetDepartments();
                        return View(vm);
                    }

                    if (!vm.IsActive)
                    {
                        ModelState.AddModelError("", "You cannot deactivate your own account.");
                        MasterDataDAL master0 = new MasterDataDAL();
                        vm.Roles = master0.GetRoles();
                        vm.Departments = master0.GetDepartments();
                        return View(vm);
                    }
                }

                UserDAL dal = new UserDAL();
                dal.UpdateUser(vm.UserId, vm.FullName, vm.MobileNumber, vm.RoleId, vm.DepartmentId, vm.IsActive, CurrentUserId);
                TempData["info"] = "User updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error updating user");
                ModelState.AddModelError("", "An error occurred.");
                MasterDataDAL master = new MasterDataDAL();
                vm.Roles = master.GetRoles();
                vm.Departments = master.GetDepartments();
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ChangeRole(int userId, int roleId)
        {
            if (userId == CurrentUserId)
            {
                return Json(new { success = false, error = "You cannot change your own role." });
            }

            var validRoles = new MasterDataDAL().GetRoles();
            if (validRoles == null || !validRoles.Any(r => r.Id == roleId))
            {
                return Json(new { success = false, error = "Invalid role." });
            }

            try
            {
                new UserDAL().ChangeUserRole(userId, roleId, CurrentUserId);
                return Json(new { success = true, message = "Role updated successfully. The user will see the change after re-login." });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error changing user role");
                return Json(new { success = false, error = "An error occurred. Please try again." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SetApproval(int userId, string approval)
        {
            if (userId == CurrentUserId)
            {
                return Json(new { success = false, error = "You cannot change your own approval status." });
            }

            byte? isApproved;
            string message;
            switch (approval)
            {
                case "approved":
                    isApproved = 1;
                    message = "User approved successfully. They can now log in.";
                    break;
                case "rejected":
                    isApproved = 0;
                    message = "User rejected successfully.";
                    break;
                case "awaiting":
                    isApproved = null;
                    message = "User set to awaiting approval.";
                    break;
                default:
                    return Json(new { success = false, error = "Invalid approval status." });
            }

            try
            {
                new UserDAL().SetUserApproval(userId, isApproved, CurrentUserId);
                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error setting user approval");
                return Json(new { success = false, error = "An error occurred." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(int userId)
        {
            if (userId == CurrentUserId)
            {
                return Json(new { success = false, error = "You cannot delete your own account." });
            }

            try
            {
                new UserDAL().DeleteUser(userId, CurrentUserId);
                return Json(new { success = true, message = "User deleted successfully." });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error deleting user");
                return Json(new { success = false, error = "An error occurred." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ToggleStatus(int userId, bool isActive)
        {
            if (userId == CurrentUserId)
            {
                return Json(new { success = false, error = "You cannot deactivate your own account." });
            }

            try
            {
                UserDAL dal = new UserDAL();
                dal.ToggleUserStatus(userId, isActive, CurrentUserId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error toggling user status");
                return Json(new { success = false, error = "An error occurred." });
            }
        }
    }
}
