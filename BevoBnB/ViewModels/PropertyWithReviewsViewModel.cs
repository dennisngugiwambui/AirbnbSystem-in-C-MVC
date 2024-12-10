namespace BevoBnB.ViewModels
{
    public class PropertyWithReviewsViewModel
    {
        public int PropertyID { get; set; }
        public string PropertyNumber { get; set; }
        public string ImageUrl { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public List<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>();

        public decimal AverageRating => (decimal)(Reviews.Count != 0 ? Reviews.Average(r => r.Rating) : 0);
        public int ReviewCount => Reviews.Count;
    }
}
