// Models/Property.cs
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BevoBnB.Models
{
    public class Property
    {
        public Property()
        {
            UnavailableDates = new HashSet<PropertyUnavailability>();
            Reservations = new HashSet<Reservation>();
            Reviews = new HashSet<Review>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PropertyID { get; set; }

        [Required]
        [ForeignKey("Host")]
        public string HostID { get; set; }

        [Required]
        [ForeignKey("Category")]
        public int CategoryID { get; set; }

        [StringLength(10)]
        [Display(Name = "Property Number")]
        public string? PropertyNumber { get; set; }

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

        [Required]
        [Display(Name = "Pets Allowed")]
        public bool PetsAllowed { get; set; }

        [Required]
        [Display(Name = "Free Parking")]
        public bool FreeParking { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Weekday Price")]
        [Range(0, 10000)]
        public decimal WeekdayPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Weekend Price")]
        [Range(0, 10000)]
        public decimal WeekendPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Cleaning Fee")]
        [Range(0, 1000)]
        public decimal CleaningFee { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100)]
        [Display(Name = "Discount Rate (%)")]
        public decimal? DiscountRate { get; set; }

        [Display(Name = "Minimum Nights for Discount")]
        [Range(1, 30)]
        public int? MinNightsForDiscount { get; set; }

        [Required]
        [Display(Name = "Property Status")]
        public bool PropertyStatus { get; set; } = true;

        [Required]
        [Display(Name = "Admin Approved")]
        public bool AdminApproved { get; set; } = false;

        [StringLength(255)]
        [Display(Name = "Property Image")]
        public string? ImageUrl { get; set; }

        // Navigation properties
        public virtual Users Host { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<PropertyUnavailability> UnavailableDates { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}