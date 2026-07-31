using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace TMS.DataAccess.ViewModels
{
    public class TicketAssignViewModel
    {
        public int TicketId { get; set; }
        public string TicketNumber { get; set; }
        public string Title { get; set; }

        [Required(ErrorMessage = "Please select a Support Executive")]
        public int AssignedToUserId { get; set; }

        public List<DropdownViewModel> SupportUsers { get; set; }
    }
}
