using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace TMS.DataAccess.ViewModels
{
    public class TicketStatusUpdateViewModel
    {
        public int TicketId { get; set; }
        public string TicketNumber { get; set; }

        [Required(ErrorMessage = "Please select a status")]
        [Display(Name = "Status")]
        public int StatusId { get; set; }

        [Required(ErrorMessage = "Please select a priority")]
        [Display(Name = "Priority")]
        public int PriorityId { get; set; }

        public List<DropdownViewModel> Statuses { get; set; }
        public List<DropdownViewModel> Priorities { get; set; }
    }
}
