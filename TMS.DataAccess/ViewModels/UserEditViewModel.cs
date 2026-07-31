using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace TMS.DataAccess.ViewModels
{
    public class UserEditViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Invalid mobile number")]
        public string MobileNumber { get; set; }

        public string Email { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; }

        public bool IsActive { get; set; }

        public List<DropdownViewModel> Roles { get; set; }
        public List<DropdownViewModel> Departments { get; set; }
    }
}
