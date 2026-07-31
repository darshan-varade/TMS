using System;
using System.ComponentModel.DataAnnotations;
namespace TMS.DataAccess.ViewModels
{
    public class CommentViewModel
    {
        public int CommentId { get; set; }
        public string Comment { get; set; }
        public bool IsInternal { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
    }

    public class CommentAddViewModel
    {
        [Required(ErrorMessage = "Comment is required")]
        public string Comment { get; set; }
        public bool IsInternal { get; set; }
    }
}
