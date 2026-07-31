using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace TMS.DataAccess.ViewModels
{
    public class TicketEditViewModel
    {
        public int TicketId { get; set; }
        public string TicketNumber { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        public int PriorityId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public int StatusId { get; set; }

        public int? AssignedToUserId { get; set; }

        public List<DropdownViewModel> Categories { get; set; }
        public List<DropdownViewModel> Priorities { get; set; }
        public List<DropdownViewModel> Statuses { get; set; }
        public List<DropdownViewModel> SupportUsers { get; set; }
    }
}
