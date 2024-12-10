using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BevoBnB.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BevoBnB.ViewModels
{
    public class PropertyDetailsViewModel
    {
        private string? category;

        public int PropertyID { get; set; }  
        public string PropertyNumber { get; set; }
        public string HostName { get; set; }
        public string Category { get => category; set => category = value; }
        public string Street { get; set; }  
        public string City { get; set; }     
        public string State { get; set; }     
        public string ImageUrl { get; set; }
        public decimal WeekdayPrice { get; set; } 
        public decimal WeekendPrice { get; set; }  
        public decimal CleaningFee { get; set; }  
        public bool AdminApproved { get; set; }    
        public bool PropertyStatus { get; set; }   
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public int Bathrooms { get; set; }
        public int Bedrooms { get; set; }
        public decimal? DiscountRate { get; set; }
        public bool FreeParking { get; set; }
        public int GuestsAllowed { get; set; }
        
        public bool PetsAllowed { get; set; }
        public int? MinNightsForDiscount { get; set; }

        public List<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>();
        //public object CheckIn { get; internal set; }
        //public object CheckOut { get; internal set; }
       // public int NumOfGuests { get; internal set; }


        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckIn { get; set; } = DateTime.Today;

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckOut { get; set; } = DateTime.Today.AddDays(1);

        [Required]
        [Range(1, 20)]
        public int NumOfGuests { get; set; }
    }


    public class CreatePropertyViewModel
    {
        [Required(ErrorMessage = "Street is required")]
        [StringLength(100)]
        public string Street { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(50)]
        public string City { get; set; }

        [Required(ErrorMessage = "State is required")]
        [StringLength(2)]
        public string State { get; set; }

        [Required(ErrorMessage = "ZIP Code is required")]
        [StringLength(10)]
        [Display(Name = "ZIP Code")]
        public string Zip { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Number of bedrooms is required")]
        [Range(1, 20)]
        public int Bedrooms { get; set; }

        [Required(ErrorMessage = "Number of bathrooms is required")]
        [Range(1, 10)]
        [Column(TypeName = "decimal(3,1)")]
        public decimal Bathrooms { get; set; }

        [Required(ErrorMessage = "Maximum number of guests is required")]
        [Range(1, 20)]
        [Display(Name = "Maximum Guests")]
        public int GuestsAllowed { get; set; }

        [Display(Name = "Pets Allowed")]
        public bool PetsAllowed { get; set; }

        [Display(Name = "Free Parking")]
        public bool FreeParking { get; set; }

        [Required(ErrorMessage = "Weekday price is required")]
        [Range(0, 10000)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal WeekdayPrice { get; set; }

        [Required(ErrorMessage = "Weekend price is required")]
        [Range(0, 10000)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal WeekendPrice { get; set; }

        [Required(ErrorMessage = "Cleaning fee is required")]
        [Range(0, 1000)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal CleaningFee { get; set; }

        [Range(0, 100)]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Discount Rate (%)")]
        public decimal? DiscountRate { get; set; }

        [Range(1, 30)]
        [Display(Name = "Minimum Nights for Discount")]
        public int? MinNightsForDiscount { get; set; }

        [Required(ErrorMessage = "Property image is required")]
        public IFormFile PropertyImage { get; set; }

        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
    }


    



}