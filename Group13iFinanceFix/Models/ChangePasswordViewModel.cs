using System.ComponentModel.DataAnnotations;

namespace Group13iFinanceFix.Models
{
    public class ChangePasswordViewModel
    {
        [Display(Name = "Current password")]
        [Required]
        public string CurrentPassword { get; set; }

        [Display(Name = "New password")]
        [Required]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm new password")]
        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
