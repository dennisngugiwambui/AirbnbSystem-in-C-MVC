using BevoBnB.Models;

namespace BevoBnB.ViewModels
{
    public class HasUserReviewedViewModel
    {
        public bool HasReviewed { get; set; }
        public int PropertyId { get; set; }
        public string CustomerId { get; set; }
    }
}