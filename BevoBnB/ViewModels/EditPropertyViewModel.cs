using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using BevoBnB.Models;

namespace BevoBnB.ViewModels
{
    public class EditPropertyViewModel
    {
        public int PropertyID { get; set; }

        [Required]
        [Display(Name = "Property Type")]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(100)]
        public string Street { get; set; }

        [Required]
        [StringLength(50)]
        public string City { get; set; }

        [Required]
        [StringLength(2)]
        public string State { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "ZIP Code")]
        public string Zip { get; set; }

        [Required]
        [Range(1, 20)]
        public int Bedrooms { get; set; }

        [Required]
        [Range(1, 10)]
        [Column(TypeName = "decimal(3,1)")]
        public decimal Bathrooms { get; set; }

        [Required]
        [Range(1, 20)]
        [Display(Name = "Maximum Guests")]
        public int GuestsAllowed { get; set; }

        [Display(Name = "Pets Allowed")]
        public bool PetsAllowed { get; set; }

        [Display(Name = "Free Parking")]
        public bool FreeParking { get; set; }

        [Required]
        [Display(Name = "Weekday Price")]
        [Range(0, 10000)]
        public decimal WeekdayPrice { get; set; }

        [Required]
        [Display(Name = "Weekend Price")]
        [Range(0, 10000)]
        public decimal WeekendPrice { get; set; }

        [Required]
        [Display(Name = "Cleaning Fee")]
        [Range(0, 1000)]
        public decimal CleaningFee { get; set; }

        [Range(0, 100)]
        [Display(Name = "Discount Rate (%)")]
        public decimal? DiscountRate { get; set; }

        [Display(Name = "Minimum Nights for Discount")]
        [Range(1, 30)]
        public int? MinNightsForDiscount { get; set; }

        [Display(Name = "Property Status")]
        public bool PropertyStatus { get; set; }

        [Display(Name = "Property Image")]
        public IFormFile PropertyImage { get; set; }

        public string ExistingImageUrl { get; set; }

        public List<SelectListItem> Categories { get; set; }

        public List<SelectListItem> States { get; set; } = new List<SelectListItem>();
    }

    

    public class ReviewViewModel
    {
        public int ReviewID { get; set; }
        public int PropertyID { get; set; }  // Add this
        public string PropertyNumber { get; set; }
        public string PropertyImageUrl { get; set; }
        public string PropertyName { get; set; }  // Add this if needed
        public string CustomerName { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; }
        public string HostComments { get; set; }
        public DateTime CreatedDate { get; set; }
        public DisputeStatus DisputeStatus { get; set; }
    }

    public class DisputeReviewViewModel
    {
        public int ReviewID { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Reason for Dispute")]
        public string DisputeReason { get; set; }
    }
}
