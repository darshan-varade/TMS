using System;
using System.Collections.Generic;
namespace TMS.DataAccess.ViewModels
{
    public class UserListViewModel
    {
        public List<UserRowViewModel> Users { get; set; }
        public int TotalRows { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string SearchTerm { get; set; }
        public int? RoleId { get; set; }

        public string SortColumn { get; set; } = "CreatedOn";
        public string SortDirection { get; set; } = "DESC";
    }

    public class UserRowViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string DepartmentName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public int TotalTickets { get; set; }
    }
}
