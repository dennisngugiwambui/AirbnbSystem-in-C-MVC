using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BevoBnB.Models;
using BevoBnB.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BevoBnB.DAL;
using Microsoft.AspNetCore.Mvc.Rendering;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BevoBnB.Controllers
{
    [Authorize(Roles = "Host")]
    public class HostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<HostController> _logger;

        public HostController(
        AppDbContext context,
        UserManager<Users> userManager,
        IWebHostEnvironment webHostEnvironment,
        ILogger<HostController> logger)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        // Add this action method to your HostController
        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var now = DateTime.Now;
                var thirtyDaysAgo = now.AddDays(-30);
                var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
                var firstDayOfLastMonth = firstDayOfMonth.AddMonths(-1);

                // Get properties with related data in one query
                var properties = await _context.Properties
                    .Include(p => p.Reviews)
                    .Include(p => p.Reservations)
                        .ThenInclude(r => r.Customer)
                    .Where(p => p.HostID == currentUser.Id)
                    .ToListAsync();

                var propertyIds = properties.Select(p => p.PropertyID).ToList();

                // Get all reservations for these properties
                var reservations = properties.SelectMany(p => p.Reservations).ToList();

                var reviews = properties.SelectMany(p => p.Reviews).ToList();

                var dashboardStats = new HostDashboardViewModel
                {
                    // Property stats
                    TotalProperties = properties.Count,
                    ActiveProperties = properties.Count(p => p.PropertyStatus && p.AdminApproved),

                    // Booking stats
                    NewBookingsCount = reservations.Count(r =>
                        r.CreatedDate >= thirtyDaysAgo &&
                        r.ReservationStatus == ReservationStatus.Confirmed),
                    TotalBookings = reservations.Count(r => r.ReservationStatus == ReservationStatus.Confirmed),

                    // Earnings calculations
                    TotalEarnings = reservations
                        .Where(r => r.ReservationStatus == ReservationStatus.Confirmed)
                        .Sum(r => r.Total),
                    MonthlyEarnings = reservations
                        .Where(r => r.CreatedDate >= firstDayOfMonth &&
                                   r.ReservationStatus == ReservationStatus.Confirmed)
                        .Sum(r => r.Total),
                    LastMonthEarnings = reservations
                        .Where(r => r.CreatedDate >= firstDayOfLastMonth &&
                                   r.CreatedDate < firstDayOfMonth &&
                                   r.ReservationStatus == ReservationStatus.Confirmed)
                        .Sum(r => r.Total),

                    // Review stats
                    AverageRating = (decimal)(reviews.Count != 0 ? Math.Round(reviews.Average(r => r.Rating), 1) : 0),
                    TotalReviews = reviews.Count,

                    // Recent bookings
                    RecentBookings = reservations
                        .Where(r => r.ReservationStatus == ReservationStatus.Confirmed)
                        .OrderByDescending(r => r.CreatedDate)
                        .Take(5)
                        .ToList()
                };

                // Calculate earnings change percentage
                if (dashboardStats.LastMonthEarnings > 0)
                {
                    dashboardStats.EarningsChange = Math.Round(
                        ((dashboardStats.MonthlyEarnings - dashboardStats.LastMonthEarnings) /
                        dashboardStats.LastMonthEarnings * 100), 1);
                }

                // Calculate performance metrics
                ViewBag.PerformanceMetrics = new
                {
                    OccupancyRate = CalculateOccupancyRate(reservations, properties),
                    AverageStayLength = CalculateAverageStayLength(reservations)
                };

                return View(dashboardStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading host dashboard for user {UserId}",
                    User.Identity.Name);
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard. Please try again.";
                return View(new HostDashboardViewModel());
            }
        }

        private double CalculateOccupancyRate(List<Reservation> reservations, List<Property> properties)
        {
            if (!properties.Any()) return 0;

            var now = DateTime.Now;
            var thirtyDaysAgo = now.AddDays(-30);
            var totalDays = 30 * properties.Count;

            var bookedDays = reservations
                .Where(r => r.ReservationStatus == ReservationStatus.Confirmed &&
                            r.CheckOut >= thirtyDaysAgo && r.CheckIn <= now)
                .Sum(r => (r.CheckOut - r.CheckIn).Days);

            return Math.Round((double)bookedDays / totalDays * 100, 1);
        }

        private double CalculateAverageStayLength(List<Reservation> reservations)
        {
            var confirmedReservations = reservations
                .Where(r => r.ReservationStatus == ReservationStatus.Confirmed);

            if (!confirmedReservations.Any()) return 0;

            var totalDays = confirmedReservations.Sum(r => (r.CheckOut - r.CheckIn).Days);
            return Math.Round((double)totalDays / confirmedReservations.Count(), 1);
        }

        // Property Management Methods
        public async Task<IActionResult> Properties()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var properties = await _context.Properties
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Include(p => p.UnavailableDates)
                .Where(p => p.HostID == currentUser.Id)
                .ToListAsync();

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(properties);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProperty()
        {
            try
            {
                var viewModel = new CreatePropertyViewModel
                {
                    Categories = await _context.Categories
                        .Select(c => new SelectListItem
                        {
                            Value = c.CategoryID.ToString(),
                            Text = c.CategoryName
                        })
                        .ToListAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create property form");
                TempData["ErrorMessage"] = "Error loading form. Please try again.";
                return RedirectToAction(nameof(Properties));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProperty(CreatePropertyViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser == null)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    // Get next property number
                    var lastProperty = await _context.Properties
                        .OrderByDescending(p => p.PropertyNumber)
                        .FirstOrDefaultAsync();

                    var nextNumber = 3001;
                    if (lastProperty != null && !string.IsNullOrEmpty(lastProperty.PropertyNumber))
                    {
                        if (int.TryParse(lastProperty.PropertyNumber, out int lastNumber))
                        {
                            nextNumber = lastNumber + 1;
                        }
                    }

                    // Handle image upload
                    string uniqueFileName = null;
                    if (model.PropertyImage != null)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "properties");
                        uniqueFileName = $"{nextNumber}_{Guid.NewGuid()}{Path.GetExtension(model.PropertyImage.FileName)}";
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        Directory.CreateDirectory(uploadsFolder);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.PropertyImage.CopyToAsync(fileStream);
                        }
                    }

                    var property = new Property
                    {
                        HostID = currentUser.Id,
                        PropertyNumber = nextNumber.ToString(),
                        CategoryID = model.CategoryID,
                        Street = model.Street,
                        City = model.City,
                        State = model.State,
                        Zip = model.Zip,
                        Bedrooms = model.Bedrooms,
                        Bathrooms = model.Bathrooms,
                        GuestsAllowed = model.GuestsAllowed,
                        PetsAllowed = model.PetsAllowed,
                        FreeParking = model.FreeParking,
                        WeekdayPrice = model.WeekdayPrice,
                        WeekendPrice = model.WeekendPrice,
                        CleaningFee = model.CleaningFee,
                        DiscountRate = model.DiscountRate,
                        MinNightsForDiscount = model.MinNightsForDiscount,
                        PropertyStatus = true,
                        AdminApproved = false,
                        ImageUrl = uniqueFileName
                    };

                    _context.Properties.Add(property);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Property created successfully!";
                    return RedirectToAction(nameof(Properties));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating property");
                ModelState.AddModelError("", "Error creating property. Please try again.");
            }

            // If we got this far, something failed, redisplay form
            model.Categories = await _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryID.ToString(),
                    Text = c.CategoryName
                })
                .ToListAsync();

            return View(model);
        }

        // Message Management Methods
        public async Task<IActionResult> Messages(string filter = "all")
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var query = _context.Messages
                    .Include(m => m.Sender)
                    .Include(m => m.Receiver)
                    .Include(m => m.Property)
                    .Include(m => m.Replies)
                    .ThenInclude(r => r.Sender)
                    .Where(m => m.ReceiverID == currentUser.Id || m.SenderID == currentUser.Id)
                    .AsQueryable();

                // Apply filters
                query = filter switch
                {
                    "unread" => query.Where(m => m.Status == MessageStatus.Unread && m.ReceiverID == currentUser.Id),
                    "sent" => query.Where(m => m.SenderID == currentUser.Id),
                    "archived" => query.Where(m => m.Status == MessageStatus.Archived),
                    _ => query
                };

                var messages = await query.OrderByDescending(m => m.SentDate).ToListAsync();

                var viewModels = messages.Select(m => new MessageViewModel
                {
                    MessageID = m.MessageID,
                    SenderName = m.Sender.FullName,
                    Subject = m.Subject,
                    Content = m.Content,
                    SentDate = m.SentDate,
                    Status = m.Status,
                    PropertyNumber = m.Property?.PropertyNumber,
                    Replies = m.Replies.Select(r => new MessageReplyViewModel
                    {
                        SenderName = r.Sender.FullName,
                        Content = r.Content,
                        SentDate = r.SentDate
                    }).ToList()
                }).ToList();

                ViewBag.CurrentFilter = filter;
                return View(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading messages for user {UserId}", User.Identity.Name);
                TempData["ErrorMessage"] = "An error occurred while loading messages. Please try again.";
                return View(new List<MessageViewModel>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(CreateMessageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var message = new Message
                {
                    SenderID = currentUser.Id,
                    ReceiverID = model.ReceiverID,
                    Subject = model.Subject,
                    Content = model.Content,
                    PropertyID = model.PropertyID,
                    Status = MessageStatus.Unread,
                    SentDate = DateTime.Now
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Messages));
            }

            return BadRequest(ModelState);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(MessageReplyCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var message = await _context.Messages.FindAsync(model.MessageID);

                if (message == null)
                {
                    return NotFound();
                }

                var reply = new MessageReply
                {
                    MessageID = model.MessageID,
                    SenderID = currentUser.Id,
                    Content = model.Content,
                    SentDate = DateTime.Now
                };

                _context.MessageReplies.Add(reply);

                // Update original message status
                message.Status = MessageStatus.Read;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Messages));
            }

            return BadRequest(ModelState);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveMessage(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            message.Status = MessageStatus.Archived;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Messages));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            message.Status = MessageStatus.Read;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Messages));
        }

        [Authorize(Roles = "Host")]
        public async Task<IActionResult> Earnings()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var now = DateTime.Now;
                var thisMonth = new DateTime(now.Year, now.Month, 1);
                var lastMonth = thisMonth.AddMonths(-1);

                // Get properties and reservations in one query to improve performance
                var properties = await _context.Properties
                    .Include(p => p.Reservations)
                    .Where(p => p.HostID == currentUser.Id)
                    .ToListAsync();

                var allReservations = properties.SelectMany(p => p.Reservations).ToList();

                // Calculate earnings
                var monthlyEarnings = allReservations
                    .Where(r => r.CreatedDate >= thisMonth && r.ReservationStatus == ReservationStatus.Confirmed)
                    .Sum(r => r.Total);

                var lastMonthEarnings = allReservations
                    .Where(r => r.CreatedDate >= lastMonth && r.CreatedDate < thisMonth &&
                               r.ReservationStatus == ReservationStatus.Confirmed)
                    .Sum(r => r.Total);

                var viewModel = new EarningsViewModel
                {
                    TotalEarnings = allReservations
                        .Where(r => r.ReservationStatus == ReservationStatus.Confirmed)
                        .Sum(r => r.Total),
                    MonthlyEarnings = monthlyEarnings,
                    LastMonthEarnings = lastMonthEarnings,
                    EarningsChange = lastMonthEarnings > 0
                        ? ((monthlyEarnings - lastMonthEarnings) / lastMonthEarnings) * 100
                        : 0,
                    TotalBookings = allReservations.Count(r => r.ReservationStatus == ReservationStatus.Confirmed),
                    CompletedBookings = allReservations.Count(r => r.CheckOut < now &&
                                                                 r.ReservationStatus == ReservationStatus.Confirmed),
                    PropertiesEarnings = properties.Select(p => new PropertyEarningsViewModel
                    {
                        PropertyID = p.PropertyID,
                        PropertyNumber = p.PropertyNumber,
                        TotalEarnings = p.Reservations
                            .Where(r => r.ReservationStatus == ReservationStatus.Confirmed)
                            .Sum(r => r.Total),
                        CompletedBookings = p.Reservations
                            .Count(r => r.CheckOut < now && r.ReservationStatus == ReservationStatus.Confirmed)
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading earnings for user {UserId}",
                    User.Identity.Name);
                TempData["ErrorMessage"] = "An error occurred while loading earnings data. Please try again.";
                return View(new EarningsViewModel());
            }
        }
        [HttpGet]
  
        public async Task<IActionResult> EditProperty(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var property = await _context.Properties
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.PropertyID == id && p.HostID == currentUser.Id);

                if (property == null)
                {
                    return NotFound();
                }

                // Get states from database
                var states = await _context.Set<State>()
                    .OrderBy(s => s.StateName)
                    .Select(s => new SelectListItem
                    {
                        Value = s.StateCode,
                        Text = s.StateName,
                        Selected = s.StateCode == property.State
                    })
                    .ToListAsync();

                var viewModel = new EditPropertyViewModel
                {
                    PropertyID = property.PropertyID,
                    CategoryID = property.CategoryID,
                    Street = property.Street,
                    City = property.City,
                    State = property.State,
                    Zip = property.Zip,
                    Bedrooms = property.Bedrooms,
                    Bathrooms = property.Bathrooms,
                    GuestsAllowed = property.GuestsAllowed,
                    PetsAllowed = property.PetsAllowed,
                    FreeParking = property.FreeParking,
                    WeekdayPrice = property.WeekdayPrice,
                    WeekendPrice = property.WeekendPrice,
                    CleaningFee = property.CleaningFee,
                    DiscountRate = property.DiscountRate,
                    MinNightsForDiscount = property.MinNightsForDiscount,
                    PropertyStatus = property.PropertyStatus,
                    ExistingImageUrl = property.ImageUrl,

                    // Add Categories
                    Categories = await _context.Categories
                        .Select(c => new SelectListItem
                        {
                            Value = c.CategoryID.ToString(),
                            Text = c.CategoryName,
                            Selected = c.CategoryID == property.CategoryID
                        })
                        .ToListAsync(),

                    // Add States from database
                    States = states
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading property for editing");
                TempData["ErrorMessage"] = "Error loading property. Please try again.";
                return RedirectToAction(nameof(Properties));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProperty(EditPropertyViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    var property = await _context.Properties
                        .FirstOrDefaultAsync(p => p.PropertyID == model.PropertyID && p.HostID == currentUser.Id);

                    if (property == null)
                    {
                        return NotFound();
                    }

                    // Handle image update
                    if (model.PropertyImage != null)
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(property.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "properties", property.ImageUrl);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Save new image
                        string uniqueFileName = $"{property.PropertyNumber}_{Guid.NewGuid()}{Path.GetExtension(model.PropertyImage.FileName)}";
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "properties");
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        Directory.CreateDirectory(uploadsFolder);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.PropertyImage.CopyToAsync(fileStream);
                        }

                        property.ImageUrl = uniqueFileName;
                    }

                    // Update property details
                    property.CategoryID = model.CategoryID;
                    property.Street = model.Street;
                    property.City = model.City;
                    property.State = model.State;
                    property.Zip = model.Zip;
                    property.Bedrooms = model.Bedrooms;
                    property.Bathrooms = model.Bathrooms;
                    property.GuestsAllowed = model.GuestsAllowed;
                    property.PetsAllowed = model.PetsAllowed;
                    property.FreeParking = model.FreeParking;
                    property.WeekdayPrice = model.WeekdayPrice;
                    property.WeekendPrice = model.WeekendPrice;
                    property.CleaningFee = model.CleaningFee;
                    property.DiscountRate = model.DiscountRate;
                    property.MinNightsForDiscount = model.MinNightsForDiscount;
                    property.PropertyStatus = model.PropertyStatus;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Property updated successfully!";
                    return RedirectToAction(nameof(Properties));
                }

                // If we got this far, something failed, redisplay form
                model.Categories = await _context.Categories
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryID.ToString(),
                        Text = c.CategoryName
                    })
                    .ToListAsync();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating property");
                TempData["ErrorMessage"] = "Error updating property. Please try again.";
                ModelState.AddModelError("", "Error updating property. Please try again.");

                model.Categories = await _context.Categories
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryID.ToString(),
                        Text = c.CategoryName
                    })
                    .ToListAsync();

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Bookings()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                // Get all properties with their reservations
                var propertiesWithReservations = await _context.Properties
                    .Include(p => p.Reservations)
                        .ThenInclude(r => r.Customer)
                    .Where(p => p.HostID == currentUser.Id)
                    .Select(p => new PropertyReservationsViewModel
                    {
                        PropertyID = p.PropertyID,
                        PropertyNumber = p.PropertyNumber,
                        ImageUrl = p.ImageUrl,
                        Street = p.Street,
                        City = p.City,
                        State = p.State,
                        Reservations = p.Reservations
                            .Select(r => new BookingViewModel
                            {
                                ReservationID = r.ReservationID,
                                ConfirmationNumber = r.ConfirmationNumber,
                                CustomerName = r.Customer.FullName,
                                CustomerEmail = r.Customer.Email,
                                CustomerPhone = r.Customer.Phone,
                                CheckIn = r.CheckIn,
                                CheckOut = r.CheckOut,
                                NumOfGuests = r.NumOfGuests,
                                SubTotal = r.SubTotal,
                                DiscountAmount = r.DiscountAmount,
                                TaxAmount = r.TaxAmount,
                                Total = r.Total,
                                Status = r.ReservationStatus,
                                CreatedDate = r.CreatedDate
                            })
                            .OrderByDescending(r => r.CheckIn)
                            .ToList()
                    })
                    .ToListAsync();

                return View(propertiesWithReservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reservations for host");
                TempData["ErrorMessage"] = "Error loading reservations. Please try again.";
                return View(new List<PropertyReservationsViewModel>());
            }
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Reviews()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                // First get all properties for this host with their reviews
                var properties = await _context.Properties
                    .Include(p => p.Reviews)
                        .ThenInclude(r => r.Customer)
                    .Where(p => p.HostID == currentUser.Id)
                    .Select(p => new PropertyWithReviewsViewModel
                    {
                        PropertyID = p.PropertyID,
                        PropertyNumber = p.PropertyNumber,
                        ImageUrl = p.ImageUrl,
                        Street = p.Street,
                        City = p.City,
                        State = p.State,
                        Reviews = p.Reviews.Select(r => new ReviewViewModel
                        {
                            ReviewID = r.ReviewID,
                            CustomerName = r.Customer.FullName,
                            Rating = r.Rating,
                            ReviewText = r.ReviewText,
                            HostComments = r.HostComments,
                            CreatedDate = r.CreatedDate,
                            DisputeStatus = r.DisputeStatus
                        }).OrderByDescending(r => r.CreatedDate).ToList()
                    })
                    .ToListAsync();

                return View(properties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reviews for host");
                TempData["ErrorMessage"] = "Error loading reviews. Please try again.";
                return View(new List<PropertyWithReviewsViewModel>());
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisputeReview(int id, string disputeReason)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var review = await _context.Reviews
                    .Include(r => r.Property)
                    .FirstOrDefaultAsync(r => r.ReviewID == id && r.Property.HostID == currentUser.Id);

                if (review == null)
                {
                    return NotFound();
                }

                review.DisputeStatus = DisputeStatus.Disputed;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Review disputed successfully.";
                return RedirectToAction(nameof(Reviews));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disputing review");
                TempData["ErrorMessage"] = "Error disputing review. Please try again.";
                return RedirectToAction(nameof(Reviews));
            }
        }
    }
}