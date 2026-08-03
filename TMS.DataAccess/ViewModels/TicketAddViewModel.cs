using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace TMS.DataAccess.ViewModels
{
    public class TicketAddViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        [Display(Name = "Priority")]
        public int PriorityId { get; set; }

        public List<DropdownViewModel> Categories { get; set; }
        public List<DropdownViewModel> Priorities { get; set; }
    }
}
