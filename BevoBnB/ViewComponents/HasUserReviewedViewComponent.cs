using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BevoBnB.Data;
using BevoBnB.Models;
using BevoBnB.DAL;

namespace BevoBnB.ViewComponents
{
    public class HasUserReviewedViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public HasUserReviewedViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int propertyId, string customerId)
        {
            var hasReviewed = await _context.Reviews
                .AnyAsync(r => r.PropertyID == propertyId &&
                              r.CustomerID == customerId &&
                              r.DisputeStatus != DisputeStatus.Removed);

            return View(hasReviewed);
        }
    }
}