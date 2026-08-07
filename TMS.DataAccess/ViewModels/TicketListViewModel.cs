using System;
using System.Collections.Generic;
namespace TMS.DataAccess.ViewModels
{
    public class TicketListViewModel
    {
        public List<TicketRowViewModel> Tickets { get; set; }
        public int TotalRows { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string SearchTerm { get; set; }
        public int? StatusId { get; set; }
        public int? PriorityId { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public string SortColumn { get; set; } = "CreatedOn";
        public string SortDirection { get; set; } = "DESC";
        public bool UnassignedOnly { get; set; }
        public int? AssignedToUserId { get; set; }

        public List<DropdownViewModel> Statuses { get; set; }
        public List<DropdownViewModel> Priorities { get; set; }
        public List<DropdownViewModel> Categories { get; set; }
        public List<DropdownViewModel> SupportUsers { get; set; }
    }

    public class TicketRowViewModel
    {
        public int TicketId { get; set; }
        public string TicketNumber { get; set; }
        public string Title { get; set; }
        public string CategoryName { get; set; }
        public string PriorityName { get; set; }
        public string StatusName { get; set; }
        public string CreatedByName { get; set; }
        public int CreatedBy { get; set; }
        public int? AssignedToUserId { get; set; }
        public string AssignedToName { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ConversationCount { get; set; }
    }
}
