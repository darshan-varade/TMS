using System;
using System.Collections.Generic;
namespace TMS.DataAccess.ViewModels
{
    public class TicketDetailViewModel
    {
        public int TicketId { get; set; }
        public string TicketNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int PriorityId { get; set; }
        public string PriorityName { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public int? AssignedToUserId { get; set; }
        public string AssignedToName { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ResolvedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedByName { get; set; }
        public int CreatedByUserId { get; set; }

        public List<CommentViewModel> Comments { get; set; }
        public List<ActivityViewModel> Activities { get; set; }
        public List<AttachmentViewModel> Attachments { get; set; }
    }
}
