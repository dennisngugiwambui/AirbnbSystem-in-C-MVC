using BevoBnB.Models;
using System.ComponentModel.DataAnnotations;

namespace BevoBnB.ViewModels
{
    public class SettingsViewModel
    {
        public Users User { get; set; }

        // Password change fields
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        // Notification preferences
        public bool EmailNotifications { get; set; }
        public bool MarketingEmails { get; set; }
    }
}
