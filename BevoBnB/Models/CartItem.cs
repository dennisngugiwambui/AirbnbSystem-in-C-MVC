namespace BevoBnB.Models
{
    public class CartItem
    {
        public int CartItemID { get; set; }
        public int PropertyID { get; set; }
        public string CustomerID { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public DateTime DateAdded { get; set; }

        public virtual Property Property { get; set; }
    }
}
