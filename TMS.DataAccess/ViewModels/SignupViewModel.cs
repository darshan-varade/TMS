using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace TMS.DataAccess.ViewModels
{
    public class SignupViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Invalid mobile number")]
        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public string PasswordHash { get; set; }
        public string OtpEmail { get; set; }
        public string OtpCode { get; set; }
        public string SignupStep { get; set; }

        public List<DropdownViewModel> Departments { get; set; }
    }
}
