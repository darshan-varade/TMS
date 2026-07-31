using System;
namespace TMS.DataAccess.Models
{
    public class TicketModel
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
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public int ConversationCount { get; set; }
    }
}
