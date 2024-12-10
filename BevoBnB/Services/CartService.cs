using BevoBnB.DAL;
using BevoBnB.Models;
using BevoBnB.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BevoBnB.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;

        public CartService(AppDbContext context)
        {
            _context = context;
        }

        private decimal CalculateTotalPrice(DateTime checkIn, DateTime checkOut, decimal weekdayPrice,
            decimal weekendPrice, decimal cleaningFee, decimal? discountRate, int? minNightsForDiscount)
        {
            decimal totalPrice = 0;
            var currentDate = checkIn;
            var numberOfNights = (checkOut - checkIn).Days;

            // Calculate base price for each day
            while (currentDate < checkOut)
            {
                if (currentDate.DayOfWeek == DayOfWeek.Friday ||
                    currentDate.DayOfWeek == DayOfWeek.Saturday)
                {
                    totalPrice += weekendPrice;
                }
                else
                {
                    totalPrice += weekdayPrice;
                }
                currentDate = currentDate.AddDays(1);
            }

            // Add cleaning fee
            totalPrice += cleaningFee;

            // Apply discount if applicable
            if (discountRate.HasValue && minNightsForDiscount.HasValue &&
                numberOfNights >= minNightsForDiscount.Value)
            {
                var discount = totalPrice * (discountRate.Value / 100m);
                totalPrice -= discount;
            }

            return totalPrice;
        }

        public async Task AddItem(CartItemViewModel item)
        {
            var cartItem = new CartItem
            {
                PropertyID = item.PropertyID,
                CustomerID = item.CustomerID,
                CheckInDate = item.CheckIn,
                CheckOutDate = item.CheckOut,
                NumberOfGuests = item.NumOfGuests,
                DateAdded = DateTime.Now
            };

            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveItem(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<CartItemViewModel>> GetCartItems(string userId)
        {
            return await _context.CartItems
                .Include(c => c.Property)
                .Where(c => c.CustomerID == userId)
                .Select(c => new CartItemViewModel
                {
                    CartItemId = c.CartItemID,
                    PropertyID = c.PropertyID,
                    PropertyName = c.Property.PropertyNumber,
                    CustomerID = c.CustomerID,
                    CheckIn = c.CheckInDate,
                    CheckOut = c.CheckOutDate,
                    NumOfGuests = c.NumberOfGuests,
                    WeekdayPrice = c.Property.WeekdayPrice,
                    WeekendPrice = c.Property.WeekendPrice,
                    CleaningFee = c.Property.CleaningFee,
                    DiscountRate = c.Property.DiscountRate,
                    MinNightsForDiscount = c.Property.MinNightsForDiscount,
                    TotalPrice = CalculateTotalPrice(
                        c.CheckInDate,
                        c.CheckOutDate,
                        c.Property.WeekdayPrice,
                        c.Property.WeekendPrice,
                        c.Property.CleaningFee,
                        c.Property.DiscountRate,
                        c.Property.MinNightsForDiscount)
                })
                .ToListAsync();
        }

        public async Task ClearCart(string userId)
        {
            var items = await _context.CartItems
                .Where(c => c.CustomerID == userId)
                .ToListAsync();

            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCartItemCount(string userId)
        {
            return await _context.CartItems
                .CountAsync(c => c.CustomerID == userId);
        }

        public Task AddItem(AddToCartViewModel model)
        {
            throw new NotImplementedException();
        }
    }
}