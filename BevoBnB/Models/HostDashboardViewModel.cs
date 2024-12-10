namespace BevoBnB.Models
{
    public class HostDashboardViewModel
    {

        public decimal TotalEarnings { get; set; }
        public decimal MonthlyEarnings { get; set; }
        public decimal LastMonthEarnings { get; set; }
        public decimal EarningsChange { get; set; }
        public int NewBookingsCount { get; set; }
        public int TotalBookings { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int ActiveProperties { get; set; }
        public int TotalProperties { get; set; }
        public List<Reservation> RecentBookings { get; set; } = new List<Reservation>();
        public int UnreadMessagesCount { get; set; }

       
    }

   


    public class PropertyEarnings
    {
        public string PropertyName { get; set; }
        public decimal TotalEarnings { get; set; }
    }

    


}
