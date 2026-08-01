using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.DataAccess.DAL;
using TMS.DataAccess.ViewModels;

namespace TMS.WebApp.Controllers
{
    public class TicketController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Tickets";
            return BuildShell(false);
        }

        public ActionResult MyAssigned()
        {
            ViewBag.Title = "My Assigned Tickets";
            return BuildShell(true);
        }

        private ActionResult BuildShell(bool myAssignedOnly)
        {
            MasterDataDAL master = new MasterDataDAL();
            ViewBag.Statuses = new SelectList(master.GetStatuses(), "Id", "Name");
            ViewBag.Priorities = new SelectList(master.GetPriorities(), "Id", "Name");
            ViewBag.Categories = new SelectList(master.GetCategories(), "Id", "Name");
            ViewBag.SupportUsers = new SelectList(new UserDAL().GetSupportUsers(), "Id", "Name");
            ViewBag.IsSupport = IsSupport;
            ViewBag.IsAdmin = IsAdmin;
            ViewBag.IsEmployee = IsEmployee;
            ViewBag.CurrentUserId = CurrentUserId;
            ViewBag.MyAssignedOnly = myAssignedOnly;

            return View("Index", new TicketListViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public ActionResult IndexPost(TicketListViewModel vm)
        {
            return BuildList(vm, null, false);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("MyAssigned")]
        public ActionResult MyAssignedPost(TicketListViewModel vm)
        {
            return BuildList(vm, CurrentUserId, true);
        }

        private ActionResult BuildList(TicketListViewModel vm, int? assignedToUserId, bool myAssignedOnly)
        {
            TicketDAL dal = new TicketDAL();

            int pageNumber = vm.PageNumber <= 0 ? 1 : vm.PageNumber;
            vm.PageSize = vm.PageSize <= 0 ? 10 : vm.PageSize;
            if (string.IsNullOrEmpty(vm.SortColumn)) vm.SortColumn = "CreatedOn";
            if (string.IsNullOrEmpty(vm.SortDirection)) vm.SortDirection = "DESC";

            int totalRows;
            vm.Tickets = dal.GetTicketList(CurrentUserId, GetNormalizedRoleName(), vm.SearchTerm, vm.StatusId, vm.PriorityId, vm.CategoryId, vm.DateFrom, vm.DateTo, vm.SortColumn, vm.SortDirection, pageNumber, vm.PageSize, out totalRows, assignedToUserId);
            vm.TotalRows = totalRows;
            vm.PageNumber = pageNumber;

            ViewBag.SupportUsers = new SelectList(new UserDAL().GetSupportUsers(), "Id", "Name");
            ViewBag.IsSupport = IsSupport;
            ViewBag.IsAdmin = IsAdmin;
            ViewBag.IsEmployee = IsEmployee;
            ViewBag.CurrentUserId = CurrentUserId;
            ViewBag.MyAssignedOnly = myAssignedOnly;

            return PartialView("_TicketListPartial", vm);
        }

        public ActionResult Create()
        {
            if (IsSupport)
            {
                TempData["info"] = "Support Executive cannot raise tickets.";
                return RedirectToAction("Index");
            }

            ViewBag.Title = "Create Ticket";
            MasterDataDAL master = new MasterDataDAL();
            var vm = new TicketAddViewModel
            {
                Categories = master.GetCategories(),
                Priorities = master.GetPriorities()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TicketAddViewModel vm)
        {
            if (IsSupport)
            {
                TempData["info"] = "Support Executive cannot raise tickets.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                MasterDataDAL master = new MasterDataDAL();
                vm.Categories = master.GetCategories();
                vm.Priorities = master.GetPriorities();
                return View(vm);
            }

            string fileError = null;
            if (Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                fileError = ValidateFile(Request.Files[0]);

            if (fileError != null)
            {
                ModelState.AddModelError("", fileError);
                MasterDataDAL master = new MasterDataDAL();
                vm.Categories = master.GetCategories();
                vm.Priorities = master.GetPriorities();
                return View(vm);
            }

            try
            {
                TicketDAL dal = new TicketDAL();
                int ticketId = dal.CreateTicket(CurrentUserId, vm.Title, vm.Description, vm.CategoryId, vm.PriorityId);

                if (Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    SaveAttachment(Request.Files[0], ticketId);
                }

                TempData["info"] = "Ticket created successfully.";
                return RedirectToAction("Details", new { id = ticketId });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error creating ticket");
                ModelState.AddModelError("", "An error occurred. Please try again.");
                MasterDataDAL master = new MasterDataDAL();
                vm.Categories = master.GetCategories();
                vm.Priorities = master.GetPriorities();
                return View(vm);
            }
        }

        public ActionResult Details(int id)
        {
            ViewBag.Title = "Ticket Details";
            TicketDAL dal = new TicketDAL();
            MasterDataDAL master = new MasterDataDAL();

            var ticket = dal.GetTicketById(id);
            if (ticket == null)
            {
                TempData["info"] = "Ticket not found.";
                return RedirectToAction("Index");
            }

            if (!CanAccess(ticket))
            {
                TempData["info"] = "You do not have access to this ticket.";
                return RedirectToAction("Index");
            }

            var vm = new TicketDetailViewModel
            {
                TicketId = ticket.TicketId,
                TicketNumber = ticket.TicketNumber,
                Title = ticket.Title,
                Description = ticket.Description,
                CategoryId = ticket.CategoryId,
                CategoryName = ticket.CategoryName,
                PriorityId = ticket.PriorityId,
                PriorityName = ticket.PriorityName,
                StatusId = ticket.StatusId,
                StatusName = ticket.StatusName,
                AssignedToUserId = ticket.AssignedToUserId,
                AssignedToName = ticket.AssignedToName,
                DueDate = ticket.DueDate,
                ResolvedOn = ticket.ResolvedOn,
                CreatedOn = ticket.CreatedOn,
                CreatedByName = ticket.CreatedByName,
                CreatedByUserId = ticket.CreatedBy,
                Comments = dal.GetComments(id),
                Activities = IsEmployee ? null : dal.GetActivities(id),
                Attachments = dal.GetAttachments(id)
            };

            ViewBag.Statuses = new SelectList(master.GetStatuses(), "Id", "Name", ticket.StatusId);
            ViewBag.Priorities = new SelectList(master.GetPriorities(), "Id", "Name", ticket.PriorityId);
            ViewBag.Categories = new SelectList(master.GetCategories(), "Id", "Name", ticket.CategoryId);
            ViewBag.SupportUsers = new SelectList(new UserDAL().GetSupportUsers(), "Id", "Name", ticket.AssignedToUserId);
            ViewBag.CurrentUserId = CurrentUserId;
            ViewBag.IsAdmin = IsAdmin;
            ViewBag.IsSupport = IsSupport;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(TicketEditViewModel vm)
        {
            if (IsSupport)
            {
                TempData["info"] = "Support Executive cannot edit tickets.";
                return RedirectToAction("Details", new { id = vm.TicketId });
            }

            try
            {
                TicketDAL dal = new TicketDAL();
                var current = dal.GetTicketById(vm.TicketId);
                if (current == null)
                {
                    TempData["info"] = "Ticket not found.";
                    return RedirectToAction("Index");
                }

                if (IsEmployee)
                {
                    if (current.CreatedBy != CurrentUserId)
                    {
                        TempData["info"] = "You can only edit your own tickets.";
                        return RedirectToAction("Details", new { id = vm.TicketId });
                    }

                    if (current.AssignedToUserId.HasValue)
                    {
                        TempData["info"] = "You cannot edit a ticket once it has been assigned.";
                        return RedirectToAction("Details", new { id = vm.TicketId });
                    }
                }

                dal.UpdateTicket(vm.TicketId, vm.Title, vm.Description, vm.CategoryId, vm.PriorityId, current.StatusId, current.AssignedToUserId, CurrentUserId);
                TempData["info"] = "Ticket updated successfully.";
                return RedirectToAction("Details", new { id = vm.TicketId });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error updating ticket");
                TempData["info"] = "Error updating ticket.";
                return RedirectToAction("Details", new { id = vm.TicketId });
            }
        }

        [AuthorizeRole(Role.Administrator)]
        [HttpGet]
        public ActionResult AssignPartial(int id)
        {
            TicketDAL dal = new TicketDAL();
            var ticket = dal.GetTicketById(id);
            if (ticket == null)
                return Content("<div class='alert alert-danger mb-0'>Ticket not found.</div>");

            var vm = new TicketAssignViewModel
            {
                TicketId = ticket.TicketId,
                TicketNumber = ticket.TicketNumber,
                Title = ticket.Title,
                AssignedToUserId = ticket.AssignedToUserId ?? 0,
                SupportUsers = new UserDAL().GetSupportUsers()
            };

            return PartialView("_AssignPartial", vm);
        }

        [AuthorizeRole(Role.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Assign(TicketAssignViewModel vm)
        {
            if (vm.AssignedToUserId <= 0)
            {
                return Json(new { success = false, message = "Please select a support executive." });
            }

            if (!ModelState.IsValid)
            {
                vm.SupportUsers = new UserDAL().GetSupportUsers();
                return PartialView("_AssignPartial", vm);
            }

            try
            {
                new TicketDAL().AssignTicket(vm.TicketId, vm.AssignedToUserId, CurrentUserId);
                return Json(new { success = true, message = "Ticket assigned successfully." });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error assigning ticket");
                return Json(new { success = false, message = "Error assigning ticket. Please try again." });
            }
        }

        [AuthorizeRole(Role.Administrator, Role.SupportExecutive)]
        [HttpGet]
        public ActionResult UpdateStatusPartial(int id)
        {
            TicketDAL dal = new TicketDAL();
            MasterDataDAL master = new MasterDataDAL();
            var ticket = dal.GetTicketById(id);
            if (ticket == null)
                return Content("<div class='alert alert-danger mb-0'>Ticket not found.</div>");

            if (IsSupport && ticket.AssignedToUserId != CurrentUserId)
                return Content("<div class='alert alert-danger mb-0'>You can only update tickets assigned to you.</div>");

            var vm = new TicketStatusUpdateViewModel
            {
                TicketId = ticket.TicketId,
                TicketNumber = ticket.TicketNumber,
                StatusId = ticket.StatusId,
                PriorityId = ticket.PriorityId,
                Statuses = master.GetStatuses(),
                Priorities = master.GetPriorities()
            };

            return PartialView("_UpdateStatusPartial", vm);
        }

        [AuthorizeRole(Role.Administrator, Role.SupportExecutive)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(TicketStatusUpdateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                MasterDataDAL master = new MasterDataDAL();
                vm.Statuses = master.GetStatuses();
                vm.Priorities = master.GetPriorities();
                return PartialView("_UpdateStatusPartial", vm);
            }

            try
            {
                TicketDAL dal = new TicketDAL();
                var ticket = dal.GetTicketById(vm.TicketId);
                if (ticket == null || !CanAccess(ticket))
                {
                    return Json(new { success = false, message = "You do not have access to this ticket." });
                }

                dal.UpdateTicketStatus(vm.TicketId, vm.StatusId, vm.PriorityId, CurrentUserId);
                return Json(new { success = true, message = "Ticket status updated successfully." });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error updating ticket status");
                return Json(new { success = false, message = "Error updating ticket status. Please try again." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddComment(int ticketId, string comment, bool isInternal = false, HttpPostedFileBase file = null)
        {
            TicketDAL dal = new TicketDAL();
            var ticket = dal.GetTicketById(ticketId);
            if (ticket == null || !CanAccess(ticket))
            {
                TempData["info"] = "You do not have access to this ticket.";
                return RedirectToAction("Details", new { id = ticketId });
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["info"] = "Comment cannot be empty.";
                return RedirectToAction("Details", new { id = ticketId });
            }

            if (file != null && file.ContentLength > 0)
            {
                string fileError = ValidateFile(file);
                if (fileError != null)
                {
                    TempData["info"] = fileError;
                    return RedirectToAction("Details", new { id = ticketId });
                }
            }

            if (!IsAdmin && !IsSupport)
                isInternal = false;

            try
            {
                int commentId = dal.AddComment(ticketId, CurrentUserId, comment, isInternal);

                if (file != null && file.ContentLength > 0)
                {
                    string storedFileName = StoreFile(file);
                    dal.AddAttachment(ticketId, CurrentUserId, storedFileName, file.FileName, Path.GetExtension(file.FileName), file.ContentType, file.ContentLength);
                }

                TempData["info"] = "Comment added.";
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error adding comment");
                TempData["info"] = "Error adding comment.";
            }

            return RedirectToAction("Details", new { id = ticketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadAttachment(int ticketId)
        {
            TicketDAL dal = new TicketDAL();
            var ticket = dal.GetTicketById(ticketId);
            if (ticket == null || !CanAccess(ticket))
            {
                TempData["info"] = "You do not have access to this ticket.";
                return RedirectToAction("Details", new { id = ticketId });
            }

            try
            {
                if (Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    var file = Request.Files[0];
                    string fileError = ValidateFile(file);
                    if (fileError != null)
                    {
                        TempData["info"] = fileError;
                        return RedirectToAction("Details", new { id = ticketId });
                    }

                    string storedFileName = StoreFile(file);
                    dal.AddAttachment(ticketId, CurrentUserId, storedFileName, file.FileName, Path.GetExtension(file.FileName), file.ContentType, file.ContentLength);
                    TempData["info"] = "Attachment uploaded.";
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error uploading attachment");
                TempData["info"] = "Error uploading attachment.";
            }

            return RedirectToAction("Details", new { id = ticketId });
        }

        [AuthorizeRole(Role.Administrator, Role.Employee)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            string message;
            bool success = false;
            try
            {
                TicketDAL dal = new TicketDAL();
                var ticket = dal.GetTicketById(id);
                if (ticket == null)
                {
                    message = "Ticket not found.";
                }
                else if (IsEmployee && (ticket.CreatedBy != CurrentUserId || ticket.AssignedToUserId.HasValue))
                {
                    message = "You can only delete your own tickets before they are assigned.";
                }
                else
                {
                    dal.DeleteTicket(id, CurrentUserId);
                    success = true;
                    message = "Ticket deleted.";
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error deleting ticket");
                message = "Error deleting ticket.";
            }

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = success, message = message });
            }

            TempData["info"] = message;
            return RedirectToAction("Index");
        }

        public ActionResult DownloadFile(int attachmentId)
        {
            try
            {
                TicketDAL dal = new TicketDAL();
                var att = dal.GetAttachmentById(attachmentId);
                if (att == null)
                {
                    TempData["info"] = "File not found.";
                    return RedirectToAction("Index");
                }

                var ticket = dal.GetTicketById(att.TicketId);
                if (ticket == null || !CanAccess(ticket))
                {
                    TempData["info"] = "You do not have access to this file.";
                    return RedirectToAction("Index");
                }

                string safeFileName = Path.GetFileName(att.StoredFileName);
                if (string.IsNullOrEmpty(safeFileName))
                {
                    TempData["info"] = "File not found.";
                    return RedirectToAction("Index");
                }

                string uploadDir = Server.MapPath("~/Content/Uploads/Tickets");
                string fullPath = Path.Combine(uploadDir, safeFileName);
                if (!System.IO.File.Exists(fullPath))
                {
                    TempData["info"] = "File not found.";
                    return RedirectToAction("Index");
                }

                string contentType = string.IsNullOrWhiteSpace(att.ContentType) ? GetContentType(att.OriginalFileName) : att.ContentType;
                return File(fullPath, contentType, att.OriginalFileName);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error downloading file");
                TempData["info"] = "Error downloading file.";
                return RedirectToAction("Index");
            }
        }

        private void SaveAttachment(HttpPostedFileBase file, int ticketId)
        {
            string storedFileName = StoreFile(file);
            new TicketDAL().AddAttachment(ticketId, CurrentUserId, storedFileName, file.FileName, Path.GetExtension(file.FileName), file.ContentType, file.ContentLength);
        }

        private string StoreFile(HttpPostedFileBase file)
        {
            string uploadDir = Server.MapPath("~/Content/Uploads/Tickets");
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            string ext = Path.GetExtension(file.FileName);
            string storedName = Guid.NewGuid().ToString("N") + ext;
            string filePath = Path.Combine(uploadDir, storedName);
            file.SaveAs(filePath);
            return storedName;
        }

        private static readonly string[] AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".doc", ".docx" };
        private const int MaxFileSizeBytes = 5 * 1024 * 1024;

        private bool CanAccess(TMS.DataAccess.Models.TicketModel ticket)
        {
            if (ticket == null) return false;
            if (IsAdmin) return true;
            if (IsSupport) return ticket.AssignedToUserId == CurrentUserId;
            return ticket.CreatedBy == CurrentUserId;
        }

        private string ValidateFile(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength <= 0) return null;

            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return "Only images, PDF and DOC/DOCX files are allowed.";

            if (file.ContentLength > MaxFileSizeBytes)
                return "File size must be 5 MB or less.";

            return null;
        }

        private string GetContentType(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLower();
            switch (ext)
            {
                case ".pdf": return "application/pdf";
                case ".doc": return "application/msword";
                case ".docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".xls": return "application/vnd.ms-excel";
                case ".xlsx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".png": return "image/png";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                case ".txt": return "text/plain";
                case ".zip": return "application/zip";
                case ".rar": return "application/x-rar-compressed";
                default: return "application/octet-stream";
            }
        }
    }
}
