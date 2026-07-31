using System.ComponentModel.DataAnnotations;
namespace TMS.DataAccess.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Invalid mobile number")]
        public string MobileNumber { get; set; }

        public string Email { get; set; }
        public string DepartmentName { get; set; }
        public string RoleName { get; set; }
    }
}
