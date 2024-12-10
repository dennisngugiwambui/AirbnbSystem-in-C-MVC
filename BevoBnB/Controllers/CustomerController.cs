using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BevoBnB.Models;
using BevoBnB.Data;
using BevoBnB.DAL;
using Microsoft.AspNetCore.Authorization;
using BevoBnB.ViewModels;
using System.Text.Json;
using BevoBnB.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors.Infrastructure;
using BevoBnB.Services;


[Authorize(Roles = "Customer")]
public class CustomerController : Controller
{
    private readonly ILogger<CustomerController> _logger; 
    private readonly AppDbContext _context;
    private readonly UserManager<Users> _userManager;

    private readonly IBookingService _bookingService;
    private readonly ICartService _cartService;


    public CustomerController(
       AppDbContext context,
       UserManager<Users> userManager,
       IBookingService bookingService,
       ICartService cartService,
       ILogger<CustomerController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _bookingService = bookingService;
        _cartService = cartService;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var now = DateTime.Now;
        var lastMonth = now.AddMonths(-1);

        // Active Bookings
        ViewBag.ActiveBookings = await _context.Reservations
            .CountAsync(r => r.CustomerID == currentUser.Id && r.CheckOut > now);

        var lastMonthBookings = await _context.Reservations
            .CountAsync(r => r.CustomerID == currentUser.Id && r.CheckOut > lastMonth && r.CheckOut <= now);
        ViewBag.BookingsIncrease = ViewBag.ActiveBookings - lastMonthBookings;

        // Total Spent
        var currentMonthReservations = await _context.Reservations
            .Where(r => r.CustomerID == currentUser.Id && r.CheckIn >= lastMonth && r.CheckIn <= now)
            .ToListAsync();

        var previousMonthReservations = await _context.Reservations
            .Where(r => r.CustomerID == currentUser.Id &&
                       r.CheckIn >= lastMonth.AddMonths(-1) &&
                       r.CheckIn < lastMonth)
            .ToListAsync();

        ViewBag.TotalSpent = currentMonthReservations.Sum(r => r.WeekdayPrice + r.WeekendPrice + r.CleaningFee -
            (r.DiscountRate.HasValue ? (r.WeekdayPrice + r.WeekendPrice + r.CleaningFee) * (r.DiscountRate.Value / 100) : 0) +
            ((r.WeekdayPrice + r.WeekendPrice + r.CleaningFee) * r.TAX));

        ViewBag.SpendingIncrease = ViewBag.TotalSpent - previousMonthReservations.Sum(r =>
            r.WeekdayPrice + r.WeekendPrice + r.CleaningFee -
            (r.DiscountRate.HasValue ? (r.WeekdayPrice + r.WeekendPrice + r.CleaningFee) * (r.DiscountRate.Value / 100) : 0) +
            ((r.WeekdayPrice + r.WeekendPrice + r.CleaningFee) * r.TAX));

        // Saved Places
        ViewBag.SavedPlaces = await _context.Properties
            .Where(p => p.Reviews.Any(r => r.CustomerID == currentUser.Id))
            .CountAsync();

        var lastMonthSavedPlaces = await _context.Properties
            .Where(p => p.Reviews.Any(r =>
                r.CustomerID == currentUser.Id &&
                EF.Functions.DateDiffDay(r.CreatedDate, now) <= 30))
            .CountAsync();

        ViewBag.NewSavedPlaces = lastMonthSavedPlaces;

        // Reviews Given
        ViewBag.ReviewsGiven = await _context.Reviews
            .CountAsync(r => r.CustomerID == currentUser.Id);

        ViewBag.NewReviews = await _context.Reviews
            .CountAsync(r => r.CustomerID == currentUser.Id &&
                           EF.Functions.DateDiffDay(r.CreatedDate, now) <= 30);

        // Upcoming Bookings
        ViewBag.UpcomingBookings = await _context.Reservations
            .Include(r => r.Property)
            .Where(r => r.CustomerID == currentUser.Id &&
                       r.CheckIn > now &&
                       r.ReservationStatus == ReservationStatus.Confirmed)
            .OrderBy(r => r.CheckIn)
            .Take(5)
            .Select(r => new {
                Id = r.ReservationID,
                PropertyName = r.Property.PropertyNumber ?? "Property",
                PropertyImage = "/api/placeholder/96/96",
                CheckIn = r.CheckIn,
                CheckOut = r.CheckOut,
                Location = $"{r.Property.City}, {r.Property.State}",
                PricePerNight = (r.WeekdayPrice + r.WeekendPrice) /
                    (r.CheckOut.Subtract(r.CheckIn).Days > 0 ? r.CheckOut.Subtract(r.CheckIn).Days : 1)
            })
            .ToListAsync();

        // Recent Activities
        var activities = new List<object>();

        // Get recent bookings
        var recentBookings = await _context.Reservations
            .Include(r => r.Property)
            .Where(r => r.CustomerID == currentUser.Id)
            .OrderByDescending(r => r.CreatedDate)
            .Take(3)
            .Select(r => new {
                Type = "booking",
                Description = $"Booking confirmed for {r.Property.PropertyNumber ?? "Property"}",
                Created = r.CreatedDate,
                IconBackground = "bg-blue-50",
                IconColor = "text-blue-600",
                IconPath = "M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
            })
            .ToListAsync();
        activities.AddRange(recentBookings);

        // Get recent reviews
        var recentReviews = await _context.Reviews
            .Include(r => r.Property)
            .Where(r => r.CustomerID == currentUser.Id)
            .OrderByDescending(r => r.CreatedDate)
            .Take(3)
            .Select(r => new {
                Type = "review",
                Description = $"Left a {r.Rating}-star review for {r.Property.PropertyNumber ?? "Property"}",
                Created = r.CreatedDate,
                IconBackground = "bg-yellow-50",
                IconColor = "text-yellow-600",
                IconPath = "M11.049 2.927c.3-.921 1.603-.921 1.902 0l1.519 4.674a1 1 0 00.95.69h4.915c.969 0 1.371 1.24.588 1.81l-3.976 2.888a1 1 0 00-.363 1.118l1.518 4.674c.3.922-.755 1.688-1.538 1.118l-3.976-2.888a1 1 0 00-1.176 0l-3.976 2.888c-.783.57-1.838-.197-1.538-1.118l1.518-4.674a1 1 0 00-.363-1.118l-3.976-2.888c-.784-.57-.38-1.81.588-1.81h4.914a1 1 0 00.951-.69l1.519-4.674z"
            })
            .ToListAsync();
        activities.AddRange(recentReviews);

        // Order activities by creation date and take most recent
        ViewBag.RecentActivities = activities
            .OrderByDescending(a => ((dynamic)a).Created)
            .Take(5)
            .Select(a => {
                var timeAgo = GetTimeAgo(((dynamic)a).Created);
                ((dynamic)a).TimeAgo = timeAgo;
                return a;
            });

        return View();
    }

    public async Task<IActionResult> MyBookings()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var bookings = await _context.Reservations
            .Include(r => r.Property)
            .Where(r => r.CustomerID == currentUser.Id)
            .OrderByDescending(r => r.CheckIn)
            .ToListAsync();

        return View(bookings);
    }

    public async Task<IActionResult> Explore(string location = null, DateTime? checkIn = null,
       DateTime? checkOut = null, int? guests = null, int? categoryId = null, string priceRange = null)
    {
        // Get all categories for the dropdown
        ViewBag.Categories = await _context.Categories.ToListAsync();

        // Base query
        var query = _context.Properties
            .Include(p => p.Category)
            .Include(p => p.Reviews)
            .Where(p => p.PropertyStatus && p.AdminApproved);

        // Apply filters if provided
        if (!string.IsNullOrEmpty(location))
        {
            query = query.Where(p => p.City.Contains(location) || p.State.Contains(location));
        }

        if (guests.HasValue)
        {
            query = query.Where(p => p.GuestsAllowed >= guests.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryID == categoryId.Value);
        }

        // Apply price range filter
        if (!string.IsNullOrEmpty(priceRange))
        {
            switch (priceRange)
            {
                case "0-100":
                    query = query.Where(p => p.WeekdayPrice < 100);
                    break;
                case "100-200":
                    query = query.Where(p => p.WeekdayPrice >= 100 && p.WeekdayPrice < 200);
                    break;
                case "200-300":
                    query = query.Where(p => p.WeekdayPrice >= 200 && p.WeekdayPrice < 300);
                    break;
                case "300+":
                    query = query.Where(p => p.WeekdayPrice >= 300);
                    break;
            }
        }

        // Check availability if dates are provided
        if (checkIn.HasValue && checkOut.HasValue)
        {
            query = query.Where(p => !p.UnavailableDates.Any(ud =>
                ud.UnavailableDate >= checkIn.Value && ud.UnavailableDate <= checkOut.Value) &&
                !p.Reservations.Any(r =>
                    (r.CheckIn <= checkOut.Value && r.CheckOut >= checkIn.Value) &&
                    r.ReservationStatus == ReservationStatus.Confirmed));
        }

        var properties = await query.ToListAsync();
        return View(properties);
    }

    public async Task<IActionResult> Favorites()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var favorites = await _context.Reviews
            .Include(r => r.Property)
            .Where(r => r.CustomerID == currentUser.Id && r.Rating >= 4)
            .Select(r => r.Property)
            .Distinct()
            .ToListAsync();

        return View(favorites);
    }

    public async Task<IActionResult> Profile()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        return View(currentUser);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(Users model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Phone = model.Phone;
            user.Address = model.Address;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        return View("Profile", model);
    }

  
    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var viewModel = new SettingsViewModel
        {
            User = user,
            EmailNotifications = true, // Get from user preferences if you have them
            MarketingEmails = true // Get from user preferences if you have them
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePassword(SettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Settings", model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var changePasswordResult = await _userManager.ChangePasswordAsync(user,
            model.CurrentPassword, model.NewPassword);

        if (!changePasswordResult.Succeeded)
        {
            foreach (var error in changePasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View("Settings", model);
        }

        // Password successfully changed
        TempData["SuccessMessage"] = "Password updated successfully";
        return RedirectToAction(nameof(Settings));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateNotificationSettings(SettingsViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        // Update user preferences here
        // You might need to create a separate table for user preferences

        TempData["SuccessMessage"] = "Notification preferences updated successfully";
        return RedirectToAction(nameof(Settings));
    }

    private string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.Now - dateTime;

        if (timeSpan.TotalMinutes < 1)
            return "just now";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minutes ago";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hours ago";
        if (timeSpan.TotalDays < 30)
            return $"{(int)timeSpan.TotalDays} days ago";
        if (timeSpan.TotalDays < 365)
            return $"{(int)(timeSpan.TotalDays / 30)} months ago";

        return $"{(int)(timeSpan.TotalDays / 365)} years ago";
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(PropertyDetailsViewModel model)
    {
        try
        {
            var today = DateTime.Today;
            DateTime checkInDate = (DateTime)model.CheckIn;
            DateTime checkOutDate = (DateTime)model.CheckOut;

            if (checkInDate < today)
            {
                TempData["ErrorMessage"] = "Check-in date cannot be in the past.";
                return RedirectToAction("PropertyDetails", new { id = model.PropertyID });
            }

            if (checkOutDate <= checkInDate)
            {
                TempData["ErrorMessage"] = "Check-out date must be after check-in date.";
                return RedirectToAction("PropertyDetails", new { id = model.PropertyID });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "Please log in to make a reservation.";
                return RedirectToAction("Login", "Account");
            }

            var cartItem = new CartItem
            {
                PropertyID = model.PropertyID,
                CustomerID = currentUser.Id,
                CheckInDate = checkInDate,
                CheckOutDate = checkOutDate,
                NumberOfGuests = model.NumOfGuests,
                DateAdded = DateTime.Now
            };

            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            // Redirect to Confirmation with the cartItem ID
            return RedirectToAction("Confirmation", new { cartItemId = cartItem.CartItemID });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to cart: {Error}", ex.Message);
            TempData["ErrorMessage"] = "An error occurred while adding to cart. Please try again.";
            return RedirectToAction("PropertyDetails", new { id = model.PropertyID });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(int cartItemId)
    {
        var cartItem = await _context.CartItems
            .Include(c => c.Property)
            .FirstOrDefaultAsync(c => c.CartItemID == cartItemId);

        if (cartItem == null)
        {
            TempData["ErrorMessage"] = "Cart item not found.";
            return RedirectToAction("Explore");
        }

        var viewModel = new ConfirmationViewModel
        {
            CartItemId = cartItemId,
            Property = cartItem.Property,
            CheckInDate = cartItem.CheckInDate,
            CheckOutDate = cartItem.CheckOutDate,
            NumberOfGuests = cartItem.NumberOfGuests,
            WeekdayPrice = cartItem.Property.WeekdayPrice,
            WeekendPrice = cartItem.Property.WeekendPrice,
            CleaningFee = cartItem.Property.CleaningFee,
            DiscountRate = cartItem.Property.DiscountRate,
            BookingDate = DateTime.Now
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmBooking(int cartItemId)
    {
        try
        {
            var cartItem = await _context.CartItems
                .Include(c => c.Property)
                .FirstOrDefaultAsync(c => c.CartItemID == cartItemId);

            if (cartItem == null)
            {
                TempData["ErrorMessage"] = "Cart item not found.";
                return RedirectToAction("Explore");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "Please log in to complete your booking.";
                return RedirectToAction("Login", "Account");
            }

            // Get the next confirmation number
            var lastConfirmationNumber = await _context.Reservations
                .OrderByDescending(r => r.ConfirmationNumber)
                .Select(r => r.ConfirmationNumber)
                .FirstOrDefaultAsync() ?? "1000";

            var nextNumber = (int.Parse(lastConfirmationNumber) + 1).ToString();

            // Create new reservation
            var reservation = new Reservation
            {
                PropertyID = cartItem.PropertyID,
                CustomerID = currentUser.Id,
                CheckIn = cartItem.CheckInDate,
                CheckOut = cartItem.CheckOutDate,
                NumOfGuests = cartItem.NumberOfGuests,
                WeekdayPrice = cartItem.Property.WeekdayPrice,
                WeekendPrice = cartItem.Property.WeekendPrice,
                CleaningFee = cartItem.Property.CleaningFee,
                DiscountRate = cartItem.Property.DiscountRate,
                ConfirmationNumber = nextNumber,
                //Status = BookingStatus.Confirmed,
                ReservationStatus = ReservationStatus.Pending,
                CreatedDate = DateTime.Now
            };

            // Add reservation and remove cart item
            _context.Reservations.Add(reservation);
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your booking has been confirmed!";
            return RedirectToAction("Success", new { confirmationNumber = nextNumber });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming booking");
            TempData["ErrorMessage"] = "An error occurred while confirming your booking. Please try again.";
            return RedirectToAction("Confirmation", new { cartItemId });
        }
    }

    [HttpGet]
    public IActionResult Success(string confirmationNumber)
    {
        var viewModel = new SuccessViewModel
        {
            ConfirmationNumber = confirmationNumber,
            BookingDate = DateTime.Now
        };

        return View(viewModel);
    }

    private decimal CalculateSubTotal(CartItem cartItem)
    {
        decimal total = 0;
        for (var date = cartItem.CheckInDate; date < cartItem.CheckOutDate; date = date.AddDays(1))
        {
            if (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday)
                total += cartItem.Property.WeekendPrice;
            else
                total += cartItem.Property.WeekdayPrice;
        }
        return total + cartItem.Property.CleaningFee;
    }

    private decimal CalculateTax(CartItem cartItem)
    {
        return CalculateSubTotal(cartItem) * 0.07m; // 7% tax
    }

    private decimal CalculateTotal(CartItem cartItem)
    {
        var subTotal = CalculateSubTotal(cartItem);
        var tax = CalculateTax(cartItem);
        return subTotal + tax;
    }
   


    [HttpGet]
        public async Task<IActionResult> Explore(string location = null, DateTime? checkIn = null,
            DateTime? checkOut = null, int? guests = null, int? categoryId = null, decimal? maxPrice = null)
        {
            try
            {
                // Get all categories for the dropdown
                ViewBag.Categories = await _context.Categories.ToListAsync();

                // Base query
                var query = _context.Properties
                    .Include(p => p.Category)
                    .Include(p => p.Reviews)
                    .Where(p => p.PropertyStatus && p.AdminApproved);

                // Apply filters
                if (!string.IsNullOrEmpty(location))
                {
                    location = location.ToLower();
                    query = query.Where(p =>
                        p.City.ToLower().Contains(location) ||
                        p.State.ToLower().Contains(location));
                }

                if (guests.HasValue)
                {
                    query = query.Where(p => p.GuestsAllowed >= guests.Value);
                }

                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryID == categoryId.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.WeekdayPrice <= maxPrice.Value);
                }

                // Check availability if dates provided
                if (checkIn.HasValue && checkOut.HasValue)
                {
                    query = query.Where(p => !p.Reservations.Any(r =>
                        r.CheckIn < checkOut.Value &&
                        r.CheckOut > checkIn.Value &&
                        r.ReservationStatus == ReservationStatus.Confirmed));
                }

                var properties = await query
                    .Select(p => new ExploreViewModel
                    {
                        PropertyID = p.PropertyID,
                        PropertyNumber = p.PropertyNumber,
                        ImageUrl = p.ImageUrl ?? "/images/placeholder.jpg",
                        Category = p.Category.CategoryName,
                        City = p.City,
                        State = p.State,
                        WeekdayPrice = p.WeekdayPrice,
                        WeekendPrice = p.WeekendPrice,
                        CleaningFee = p.CleaningFee,
                        Bedrooms = p.Bedrooms,
                        Bathrooms = (int)p.Bathrooms,
                        GuestsAllowed = p.GuestsAllowed,
                        PetsAllowed = p.PetsAllowed,
                        FreeParking = p.FreeParking,
                        DiscountRate = p.DiscountRate,
                        MinNightsForDiscount = p.MinNightsForDiscount,
                        AverageRating = (decimal)(p.Reviews.Count != 0 ?
                            p.Reviews.Average(r => r.Rating) : 0),
                        ReviewCount = p.Reviews.Count
                    })
                    .ToListAsync();

                ViewBag.SearchParams = new
                {
                    Location = location,
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    Guests = guests,
                    CategoryId = categoryId,
                    MaxPrice = maxPrice
                };

                return View(properties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Explore action");
                TempData["ErrorMessage"] = "Error loading properties.";
                return View(new List<ExploreViewModel>());
            }
        }

        [HttpGet]
        public IActionResult Cart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<Reservation>>("Cart")
                ?? new List<Reservation>();

            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Explore");
            }

            var cartItems = cart.Select(static r => new CartItemViewModel
            {
                ReservationID = r.ReservationID,
                PropertyID = r.PropertyID,
                CheckIn = r.CheckIn,
                CheckOut = r.CheckOut,
                NumOfGuests = r.NumOfGuests,
                WeekdayPrice = r.WeekdayPrice,
                WeekendPrice = r.WeekendPrice,
                CleaningFee = r.CleaningFee,
                DiscountRate = r.DiscountRate,
                SubTotal = r.SubTotal,
                DiscountAmount = r.DiscountAmount,
                TaxAmount = r.TaxAmount,
                Total = r.Total
            }).ToList();

            var viewModel = new CartViewModel
            {
                Items = cartItems,
                SubTotal = cartItems.Sum(i => i.SubTotal),
                TotalDiscounts = cartItems.Sum(i => i.DiscountAmount),
                TaxAmount = cartItems.Sum(i => i.TaxAmount),
                GrandTotal = cartItems.Sum(i => i.Total)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<Reservation>>("Cart");

            if (cart == null || !cart.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Explore");
            }

            try
            {
                // Get the next confirmation number
                var lastConfirmationNumber = await _context.Reservations
                    .OrderByDescending(r => r.ConfirmationNumber)
                    .Select(r => r.ConfirmationNumber)
                    .FirstOrDefaultAsync() ?? "21900";

                var nextNumber = int.Parse(lastConfirmationNumber) + 1;

                // Create reservations
                foreach (var reservation in cart)
                {
                    reservation.ConfirmationNumber = nextNumber.ToString();
                    reservation.ReservationStatus = ReservationStatus.Confirmed;
                    _context.Reservations.Add(reservation);
                }

                await _context.SaveChangesAsync();

                // Clear the cart
                HttpContext.Session.Remove("Cart");

                return RedirectToAction("Confirmation", new { confirmationNumber = nextNumber });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing checkout");
                TempData["ErrorMessage"] = "Error processing your reservation.";
                return RedirectToAction("Cart");
            }
        }

       
       

        
        [HttpGet]
        public async Task<IActionResult> PropertyDetails(int id)
        {
            try
            {
                var property = await _context.Properties
                    .Include(p => p.Category)
                    .Include(p => p.Host)
                    .Include(p => p.Reviews)
                        .ThenInclude(r => r.Customer)
                    .FirstOrDefaultAsync(p => p.PropertyID == id);

                if (property == null)
                {
                    TempData["ErrorMessage"] = "Property not found.";
                    return RedirectToAction("Explore");
                }

                var viewModel = new PropertyDetailsViewModel
                {
                    PropertyID = property.PropertyID,
                    PropertyNumber = property.PropertyNumber,
                    HostName = property.Host != null ? $"{property.Host.FirstName} {property.Host.LastName}" : "No Host",
                    Category = property.Category?.CategoryName ?? "Uncategorized",
                    Street = property.Street,
                    City = property.City,
                    State = property.State,
                    ImageUrl = !string.IsNullOrEmpty(property.ImageUrl) ? property.ImageUrl : "/images/placeholder.jpg",
                    WeekdayPrice = property.WeekdayPrice,
                    WeekendPrice = property.WeekendPrice,
                    CleaningFee = property.CleaningFee,
                    DiscountRate = (decimal)property.DiscountRate,
                    MinNightsForDiscount = property.MinNightsForDiscount,
                    Bedrooms = property.Bedrooms,
                    Bathrooms = (int)property.Bathrooms,
                    GuestsAllowed = property.GuestsAllowed,
                    PetsAllowed = property.PetsAllowed,
                    FreeParking = property.FreeParking,
                    Rating = property.Reviews.Any() ?
                        (decimal)property.Reviews.Average(r => r.Rating) : 0,
                    ReviewCount = property.Reviews.Count,
                    Reviews = property.Reviews.Select(r => new ReviewViewModel
                    {
                        CustomerName = $"{r.Customer.FirstName} {r.Customer.LastName}",
                        Rating = r.Rating,
                        ReviewText = r.ReviewText,
                        CreatedDate = r.CreatedDate
                    }).OrderByDescending(r => r.CreatedDate).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving property details for ID: {PropertyId}", id);
                TempData["ErrorMessage"] = "Error loading property details.";
                return RedirectToAction("Explore");
            }
        }


        private decimal CalculateStayPrice(Property property, DateTime checkIn, DateTime checkOut)
        {
            decimal total = 0;
            for (var date = checkIn; date < checkOut; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday)
                    total += property.WeekendPrice;
                else
                    total += property.WeekdayPrice;
            }
            return total;
        }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitReview(int propertyId, int rating, string reviewText)
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // Check if user has already reviewed this property
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.PropertyID == propertyId &&
                                        r.CustomerID == currentUser.Id &&
                                        r.DisputeStatus != DisputeStatus.Removed);

            if (existingReview != null)
            {
                return Json(new { success = false, message = "You have already reviewed this property." });
            }

            var review = new Review
            {
                PropertyID = propertyId,
                CustomerID = currentUser.Id,
                Rating = rating,
                ReviewText = reviewText,
                DisputeStatus = DisputeStatus.None,
                CreatedDate = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting review");
            return Json(new { success = false, message = "Error submitting review. Please try again." });
        }
    }


}