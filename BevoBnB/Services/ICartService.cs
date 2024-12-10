using BevoBnB.ViewModels;

namespace BevoBnB.Services
{
    public interface ICartService
    {
        Task AddItem(CartItemViewModel item);
        Task RemoveItem(int cartItemId);
        Task<IEnumerable<CartItemViewModel>> GetCartItems(string userId);
        Task ClearCart(string userId);
        Task<int> GetCartItemCount(string userId);
        Task AddItem(AddToCartViewModel model);
    }
}