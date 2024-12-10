using BevoBnB.Models;

namespace BevoBnB.ViewModels
{
    public class PropertyReservationsViewModel
    {
        public int PropertyID { get; set; }
        public string PropertyNumber { get; set; }
        public string ImageUrl { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public List<BookingViewModel> Reservations { get; set; } = new List<BookingViewModel>();
        public decimal TotalEarnings => Reservations.Sum(r => r.Total);
        public int ActiveReservations => Reservations.Count(r => r.Status == ReservationStatus.Confirmed
            && r.CheckOut >= DateTime.Now);
    }
}
