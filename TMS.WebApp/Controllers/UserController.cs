using System;
using System.Linq;
using System.Web.Mvc;
using TMS.DataAccess.DAL;
using TMS.DataAccess.ViewModels;

namespace TMS.WebApp.Controllers
{
    [AuthorizeRole(Role.Administrator)]
    public class UserController : BaseController
    {
        public ActionResult Index(int? page, string searchTerm, int? roleId, string sortColumn, string sortDirection)
        {
            ViewBag.Title = "Users";
            UserDAL dal = new UserDAL();
            MasterDataDAL master = new MasterDataDAL();

            int pageNumber = page ?? 1;
            if (string.IsNullOrEmpty(sortColumn)) sortColumn = "CreatedOn";
            if (string.IsNullOrEmpty(sortDirection)) sortDirection = "DESC";

            int totalRows;
            var vm = new UserListViewModel
            {
                Users = dal.GetUserList(searchTerm, roleId, sortColumn, sortDirection, pageNumber, 10, out totalRows),
                TotalRows = totalRows,
                PageNumber = pageNumber,
                PageSize = 10,
                SearchTerm = searchTerm,
                RoleId = roleId,
                SortColumn = sortColumn,
                SortDirection = sortDirection
            };

            ViewBag.Roles = new SelectList(master.GetRoles(), "Id", "Name", roleId);
            ViewBag.CurrentUserId = CurrentUserId;
            return View(vm);
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
        public JsonResult SetApproval(int userId, bool isApproved)
        {
            if (userId == CurrentUserId)
            {
                return Json(new { success = false, error = "You cannot change your own approval status." });
            }

            try
            {
                new UserDAL().SetUserApproval(userId, isApproved, CurrentUserId);
                return Json(new { success = true, message = isApproved ? "User approved successfully." : "User rejected successfully." });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error setting user approval");
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
