using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using BevoBnB.ViewModels;
using BevoBnB.Models;

namespace BevoBnB.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public decimal NewUsersPercentage { get; set; }
        public int TotalProperties { get; set; }
        public int PendingApprovals { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal AverageCommissionPerReservation { get; set; }
        public int PendingDisputes { get; set; }
        public List<PropertyViewModel> RecentProperties { get; set; }
        public List<ReviewViewModel> RecentReviews { get; set; }

        public AdminDashboardViewModel()
        {
            RecentProperties = new List<PropertyViewModel>();
            RecentReviews = new List<ReviewViewModel>();
        }
    }

    public class PropertyViewModel
    {
        public string PropertyNumber { get; set; }
        public string ImageUrl { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public decimal WeekdayPrice { get; set; }
        public bool AdminApproved { get; set; }
    }

    public class UserManagementViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserType UserType { get; set; }  // Changed from Role to UserType
        public bool HireStatus { get; set; }
        public List<string> Roles { get; set; } = new List<string>(); // Add this for Identity roles
        public string FullName => $"{FirstName} {LastName}";
        public string StatusDisplay => HireStatus ? "Active" : "Inactive";
    }

    public class CreateAdminViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Required]
        [Display(Name = "Address")]
        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        [Display(Name = "Birthday")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Birthday { get; set; }
    }

    public class PropertyManagementViewModel
    {
        public int PropertyID { get; set; }
        public string PropertyNumber { get; set; }
        public string HostName { get; set; }
        public string Category { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public bool AdminApproved { get; set; }
        public bool PropertyStatus { get; set; }
        public string ImageUrl { get; set; }
        public string ApprovalStatus => AdminApproved ? "Approved" : "Pending";
        public string StatusDisplay => PropertyStatus ? "Active" : "Inactive";
    }

    public class CategoryViewModel
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int PropertyCount { get; set; }
    }

    public class CreateCategoryViewModel
    {
        [Required]
        [Display(Name = "Category Name")]
        [StringLength(50)]
        public string CategoryName { get; set; }
    }

    public class ReviewManagementViewModel
    {
        public int ReviewID { get; set; }

        public int PropertyID { get; set; }
        public string PropertyNumber { get; set; }
        public string CustomerName { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; }
        public string HostComments { get; set; }  // Changed from HostResponse
        public DateTime CreatedDate { get; set; }
        public DisputeStatus DisputeStatus { get; set; }
        public string DisputeText { get; set; }

        // Helper Methods
        public string GetStarRating()
        {
            return string.Concat(Enumerable.Repeat("⭐", Rating));
        }

        public string GetRatingText()
        {
            return Rating switch
            {
                1 => "Poor",
                2 => "Fair",
                3 => "Average",
                4 => "Good",
                5 => "Excellent",
                _ => "Not Rated"
            };
        }

        public string GetDisputeStatusBadgeClass()
        {
            return DisputeStatus switch
            {
                DisputeStatus.Disputed => "bg-yellow-100 text-yellow-800",
                DisputeStatus.Resolved => "bg-green-100 text-green-800",
                DisputeStatus.Removed => "bg-red-100 text-red-800",
                _ => "bg-gray-100 text-gray-800"
            };
        }

        public string GetTimeAgo()
        {
            var timeSpan = DateTime.Now - CreatedDate;
            return timeSpan.TotalDays switch
            {
                <= 1 => "Today",
                <= 2 => "Yesterday",
                <= 7 => $"{timeSpan.Days} days ago",
                <= 30 => $"{timeSpan.Days / 7} weeks ago",
                <= 365 => $"{timeSpan.Days / 30} months ago",
                _ => $"{timeSpan.Days / 365} years ago"
            };
        }

        public bool IsWithinDisputePeriod()
        {
            return (DateTime.Now - CreatedDate).TotalDays <= 30;
        }

        public string GetTruncatedReviewText(int maxLength = 100)
        {
            if (string.IsNullOrEmpty(ReviewText) || ReviewText.Length <= maxLength)
                return ReviewText;
            return ReviewText.Substring(0, maxLength) + "...";
        }

        // Helper Properties
        public bool IsDisputed => DisputeStatus == DisputeStatus.Disputed;
        public bool IsResolved => DisputeStatus == DisputeStatus.Resolved;
        public bool IsRemoved => DisputeStatus == DisputeStatus.Removed;
    }

    public class AdminReportViewModel
    {
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Total Reservations")]
        public int TotalReservations { get; set; }

        [Display(Name = "Total Commission")]
        [DataType(DataType.Currency)]
        public decimal TotalCommission { get; set; }

        [Display(Name = "Total Properties")]
        public int TotalProperties { get; set; }

        [Display(Name = "Average Commission per Reservation")]
        [DataType(DataType.Currency)]
        public decimal AverageCommissionPerReservation { get; set; }

        public List<MonthlyRevenueData> MonthlyRevenue { get; set; }
        public Dictionary<string, int> ReservationsByCity { get; set; }
        public Dictionary<string, decimal> RevenueByCategory { get; set; }



        // Computed Properties
        public string DateRangeDisplay =>
            StartDate.HasValue && EndDate.HasValue
                ? $"{StartDate.Value:MM/dd/yyyy} - {EndDate.Value:MM/dd/yyyy}"
                : "All Time";



 
        public List<ReservationDetailData> DetailedReservations { get; set; }

       
    }



 
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Birthday { get; set; }

        public bool HireStatus { get; set; }
        public UserType UserType { get; set; }  // Added
        public List<string> CurrentRoles { get; set; } = new List<string>();  // Added

        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; }
    }

    public class ReviewDisputeResolutionViewModel
    {
        public int ReviewID { get; set; }

        [Required]
        [Display(Name = "Resolution Comment")]
        [StringLength(500)]
        public string ResolutionComment { get; set; }

        [Required]
        [Display(Name = "Uphold Review")]
        public bool UpholdReview { get; set; }

        // Review details for display
        public string PropertyNumber { get; set; }
        public string CustomerName { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; }
        public string HostComments { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class PropertySearchViewModel
    {
        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "State")]
        public string State { get; set; }

        [Display(Name = "Category")]
        public int? CategoryID { get; set; }

        [Display(Name = "Guest Rating")]
        public decimal? MinRating { get; set; }

        [Display(Name = "Price Range")]
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        [Display(Name = "Bedrooms")]
        public int? Bedrooms { get; set; }

        [Display(Name = "Bathrooms")]
        public int? Bathrooms { get; set; }

        [Display(Name = "Pets Allowed")]
        public bool? PetsAllowed { get; set; }

        [Display(Name = "Free Parking")]
        public bool? FreeParking { get; set; }

        public List<SelectListItem> Categories { get; set; }
        public List<SelectListItem> States { get; set; }
    }

    public class UserDetailsViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime Birthday { get; set; }
        public UserType UserType { get; set; }
        public bool HireStatus { get; set; }
        public List<string> Roles { get; set; } = new List<string>();

        // Navigation properties for related data
        public List<PropertyViewModel> Properties { get; set; } = new List<PropertyViewModel>();
        public List<PropertyReservationsViewModel> Reservations { get; set; } = new List<PropertyReservationsViewModel>();

        // Computed properties
        public string FullName => $"{FirstName} {LastName}";
        public string StatusDisplay => HireStatus ? "Active" : "Inactive";
        public int Age => DateTime.Today.Year - Birthday.Year -
            (DateTime.Today.DayOfYear < Birthday.DayOfYear ? 1 : 0);
    }

    public class PropertyManagementDetailViewModel
    {
        public int PropertyID { get; set; }
        public string PropertyNumber { get; set; }
        public string HostName { get; set; }
        public string Category { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; }
        public decimal WeekdayPrice { get; set; }
        public decimal WeekendPrice { get; set; }
        public decimal CleaningFee { get; set; }
        public decimal? DiscountRate { get; set; }
        public int? MinNightsForDiscount { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<PropertyReviewViewModel> Reviews { get; set; }
        public List<PropertyReservationViewModel> Reservations { get; set; }
    }

    public class PropertyReviewViewModel
    {
        public int ReviewID { get; set; }
        public string CustomerName { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; }
        public string HostResponse { get; set; }
        public DisputeStatus DisputeStatus { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class PropertyReservationViewModel
    {
        public int ReservationID { get; set; }
        public string CustomerName { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int NumOfGuests { get; set; }
        public ReservationStatus ReservationStatus { get; set; }
        public decimal WeekdayPrice { get; set; }
        public decimal WeekendPrice { get; set; }
        public decimal CleaningFee { get; set; }
        public decimal? DiscountRate { get; set; }
        public decimal TAX { get; set; }
        public string ConfirmationNumber { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
    }

    public class AddPropertyViewModel
    {
        [Required]
        public string PropertyNumber { get; set; }

        [Required]
        public string HostID { get; set; }

        [Required]
        public int CategoryID { get; set; }

        [Required]
        public string Street { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string Zip { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal WeekdayPrice { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal WeekendPrice { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal CleaningFee { get; set; }

        [Range(0, 100)]
        public decimal? DiscountRate { get; set; }

        public int? MinNightsForDiscount { get; set; }
    }


    

    public class MonthlyRevenueData
    {
        public string Month { get; set; }
        public decimal Revenue { get; set; }
        public int Reservations { get; set; }
    }

  
    public class ExploreViewModel
    {
        public int PropertyID { get; set; }
        public string PropertyNumber { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string HostName { get; set; }
        public decimal WeekdayPrice { get; set; }
        public decimal WeekendPrice { get; set; }
        public decimal CleaningFee { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public int GuestsAllowed { get; set; }
        public bool PetsAllowed { get; set; }
        public bool FreeParking { get; set; }
        public decimal? DiscountRate { get; set; }
        public int? MinNightsForDiscount { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; }
    }

    // Add this class to your ViewModels
    public class ReservationDetailData
    {
        public string ConfirmationNumber { get; set; }
        public string PropertyNumber { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int NumOfGuests { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; }
    }


}