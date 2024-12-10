using BevoBnB.Models;
using System.ComponentModel.DataAnnotations;

namespace BevoBnB.ViewModels
{
    public class BookingViewModel
    {
        public int ReservationID { get; set; }
        public string ConfirmationNumber { get; set; }
        public string PropertyNumber { get; set; }
        public string PropertyImageUrl { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        //public DateTime CheckIn { get; set; }
      //  public DateTime CheckOut { get; set; }
        //public int NumOfGuests { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public ReservationStatus Status { get; set; }

        public DateTime CreatedDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Check-in Date")]
        public DateTime CheckIn { get; set; } = DateTime.Today;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Check-out Date")]
        public DateTime CheckOut { get; set; } = DateTime.Today.AddDays(1);

        [Required]
        [Range(1, 20, ErrorMessage = "Number of guests must be between 1 and 20")]
        [Display(Name = "Number of Guests")]
        public int NumOfGuests { get; set; }

        public BookingFormModel BookingForm { get; set; } = new BookingFormModel();
    }

    public class BookingFormModel
    {
        public int PropertyID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Check-in Date")]
        public DateTime CheckIn { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Check-out Date")]
        public DateTime CheckOut { get; set; }

        [Required]
        [Range(1, 20, ErrorMessage = "Number of guests must be between 1 and 20")]
        [Display(Name = "Number of Guests")]
        public int NumOfGuests { get; set; }
    }
    public class ShoppingCart
    {
        public List<Reservation> Reservations { get; set; } = new List<Reservation>();
        public decimal SubTotal => Reservations.Sum(r => r.SubTotal);
        public decimal TotalDiscounts => Reservations.Sum(r => r.DiscountAmount);
        public decimal TaxAmount => (SubTotal - TotalDiscounts) * 0.07m;
        public decimal GrandTotal => SubTotal - TotalDiscounts + TaxAmount;
    }

    public class ReservationCheckoutViewModel
    {
        public List<ReservationSummaryItem> Items { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalDiscounts { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
    }

    public class ReservationSummaryItem
    {
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int NumOfGuests { get; set; }
        public decimal StayPrice { get; set; }
        public decimal CleaningFee { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
    }
    public class CartItemViewModel
    {
        public int ReservationID { get; set; }
        public int PropertyID { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int NumOfGuests { get; set; }
        public decimal WeekdayPrice { get; set; }
        public decimal WeekendPrice { get; set; }
        public decimal CleaningFee { get; set; }
        public decimal? DiscountRate { get; set; }
        public decimal SubTotal { get; set; }
       
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public object CartItemId { get; set; }
        public object PropertyName { get; set; }
  
        public int? MinNightsForDiscount { get; set; }
        public decimal TotalPrice { get; set; }
        public string CustomerID { get; internal set; }

        public decimal DiscountAmount { get; set; }
    }

    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalDiscounts { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
    }

    public class ConfirmationViewModel
    {
        public DateTime BookingDate { get; set; }
        public int CartItemId { get; set; }
        public string ConfirmationNumber { get; set; }
        public Property Property { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal WeekdayPrice { get; set; }
        public decimal WeekendPrice { get; set; }
        public decimal CleaningFee { get; set; }
        public decimal? DiscountRate { get; set; }
        public decimal TAX { get; set; } = 0.07m;

        // Calculated properties
        public decimal SubTotal => WeekdayPrice + WeekendPrice + CleaningFee;
        public decimal DiscountAmount => DiscountRate.HasValue ? (SubTotal * (DiscountRate.Value / 100)) : 0;
        public decimal TaxAmount => (SubTotal - DiscountAmount) * TAX;
        public decimal Total => SubTotal - DiscountAmount + TaxAmount;
    }

   

    public class AddToCartViewModel
    {

       
        public int PropertyID { get; set; }
        public string CustomerID { get; set; }

        
        [DataType(DataType.Date)]
        public DateTime CheckIn { get; set; }

        [DataType(DataType.Date)]
        public DateTime CheckOut { get; set; }
 
        [Range(1, 20, ErrorMessage = "Number of guests must be between 1 and 20")]
        public int NumOfGuests { get; set; }
        public decimal WeekdayPrice { get; set; }
        public decimal WeekendPrice { get; set; }
        public decimal CleaningFee { get; set; }
        public decimal? DiscountRate { get; set; }
        public int? MinNightsForDiscount { get; set; }

       
       
       

        // Calculated properties
        public decimal SubTotal
        {
            get
            {
                decimal total = 0;
                for (var date = CheckIn; date < CheckOut; date = date.AddDays(1))
                {
                    if (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday)
                        total += WeekendPrice;
                    else
                        total += WeekdayPrice;
                }
                return total + CleaningFee;
            }
        }

        public decimal DiscountAmount
        {
            get
            {
                if (DiscountRate.HasValue && MinNightsForDiscount.HasValue)
                {
                    int stayLength = (CheckOut - CheckIn).Days;
                    if (stayLength >= MinNightsForDiscount.Value)
                    {
                        return SubTotal * (DiscountRate.Value / 100m);
                    }
                }
                return 0;
            }
        }

        public decimal TaxAmount
        {
            get
            {
                const decimal TAX_RATE = 0.0825m; // 8.25% tax rate
                return (SubTotal - DiscountAmount) * TAX_RATE;
            }
        }

        public decimal Total
        {
            get
            {
                return SubTotal - DiscountAmount + TaxAmount;
            }
        }
    }

    public class SuccessViewModel
    {
        public string ConfirmationNumber { get; set; }
        public DateTime BookingDate { get; set; }

        public Property Property { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PropertyNumber { get; set; }
        

    }
}
