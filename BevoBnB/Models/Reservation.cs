
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using BevoBnB.ViewModels;

namespace BevoBnB.Models
{
    public enum ReservationStatus
    {
        Confirmed,
        Cancelled,
        Pending
    }

    public class Reservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReservationID { get; set; }

        [Required]
        [ForeignKey("Property")]
        public int PropertyID { get; set; }

        [Required]
        [ForeignKey("Customer")]
        public string CustomerID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Check-in Date")]
        public DateTime CheckIn { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Check-out Date")]
        public DateTime CheckOut { get; set; }

        [Required]
        [Range(1, 20)]
        [Display(Name = "Number of Guests")]
        public int NumOfGuests { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal WeekdayPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal WeekendPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal CleaningFee { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? DiscountRate { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TAX { get; set; } = 0.07m; // 7% tax rate

        [Required]
        [StringLength(20)]
        public string ConfirmationNumber { get; set; }

        [Required]
        public ReservationStatus ReservationStatus { get; set; } = ReservationStatus.Confirmed;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Property Property { get; set; }
        public virtual Users Customer { get; set; }

        // Calculated properties
        [NotMapped]
        public decimal SubTotal => (WeekdayPrice + WeekendPrice + CleaningFee);

        [NotMapped]
        public decimal DiscountAmount => DiscountRate.HasValue ? (SubTotal * (DiscountRate.Value / 100)) : 0;

        [NotMapped]
        public decimal TaxAmount => (SubTotal - DiscountAmount) * TAX;

        [NotMapped]
        public decimal Total => SubTotal - DiscountAmount + TaxAmount;

        //public BookingStatus Status { get; set; }
    }
}
