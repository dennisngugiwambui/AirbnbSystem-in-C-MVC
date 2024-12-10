using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BevoBnB.Models;
using BevoBnB.ViewModels;
using Microsoft.AspNetCore.Authorization;
using BevoBnB.DAL;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;

namespace BevoBnB.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            AppDbContext context,
            UserManager<Users> userManager,
            IWebHostEnvironment webHostEnvironment,
            ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        // Controllers/AdminController.cs
        public async Task<IActionResult> Index()
        {
            try
            {
                var now = DateTime.Now;
                var thirtyDaysAgo = now.AddDays(-30);

                // Get total users
                var totalUsers = await _context.Users.CountAsync();
                var usersLastMonth = await _context.Users.CountAsync(u => u.CreatedDate >= thirtyDaysAgo);
                var newUsersPercentage = totalUsers > 0 ?
                    ((decimal)usersLastMonth / totalUsers) * 100 : 0;

                // Get properties data
                var totalProperties = await _context.Properties.CountAsync();
                var pendingApprovals = await _context.Properties
                    .CountAsync(p => !p.AdminApproved);

                // Get commission data
                var confirmedReservations = await _context.Reservations
                    .Where(r => r.ReservationStatus == ReservationStatus.Confirmed)
                    .ToListAsync();
                var totalCommission = confirmedReservations.Sum(r => r.SubTotal * 0.10m);
                var averageCommission = confirmedReservations.Any() ?
                    totalCommission / confirmedReservations.Count : 0;

                // Get pending disputes
                var pendingDisputes = await _context.Reviews
                    .CountAsync(r => r.DisputeStatus == DisputeStatus.Disputed);

                // Get recent properties
                var recentProperties = await _context.Properties
                    .OrderByDescending(p => p.PropertyID)
                    .Take(3)
                    .Select(p => new PropertyViewModel
                    {
                        PropertyNumber = p.PropertyNumber,
                        ImageUrl = p.ImageUrl ?? "/images/placeholder.jpg",
                        City = p.City,
                        State = p.State,
                        WeekdayPrice = p.WeekdayPrice,
                        AdminApproved = p.AdminApproved
                    })
                    .ToListAsync();

                // Get recent reviews
                var recentReviews = await _context.Reviews
                    .Include(r => r.Customer)
                    .Include(r => r.Property)
                    .OrderByDescending(r => r.CreatedDate)
                    .Take(3)
                    .Select(r => new ReviewViewModel
                    {
                        PropertyNumber = r.Property.PropertyNumber,
                        CustomerName = $"{r.Customer.FirstName} {r.Customer.LastName}",
                        Rating = r.Rating,
                        ReviewText = r.ReviewText,
                        CreatedDate = r.CreatedDate
                    })
                    .ToListAsync();

                var viewModel = new AdminDashboardViewModel
                {
                    TotalUsers = totalUsers,
                    NewUsersPercentage = Math.Round(newUsersPercentage, 1),
                    TotalProperties = totalProperties,
                    PendingApprovals = pendingApprovals,
                    TotalCommission = totalCommission,
                    AverageCommissionPerReservation = averageCommission,
                    PendingDisputes = pendingDisputes,
                    RecentProperties = recentProperties,
                    RecentReviews = recentReviews
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading admin dashboard");
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard.";
                return View(new AdminDashboardViewModel());
            }
        }

        #region User Management

        public async Task<IActionResult> ManageUsers()
        {
            try
            {
                // Get users with their roles
                var users = await _context.Users
                    .Select(u => new UserManagementViewModel
                    {
                        Id = u.Id,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        UserType = u.UserType,  // Changed from Role to UserType
                        HireStatus = u.HireStatus,
                        Roles = _userManager.GetRolesAsync(u).Result.ToList() // Add roles from Identity
                    })
                    .ToListAsync();

                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users list");
                TempData["ErrorMessage"] = "Error loading users. Please try again.";
                return View(new List<UserManagementViewModel>());
            }
        }

        [HttpGet]
        public IActionResult CreateAdmin()
        {
            return View(new CreateAdminViewModel());
        }
       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(CreateAdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new Users
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Phone = model.Phone,
                    Address = model.Address,
                    Birthday = model.Birthday,
                    HireStatus = true,
                    UserType = UserType.Admin
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                    TempData["SuccessMessage"] = "Admin created successfully.";
                    return RedirectToAction(nameof(ManageUsers));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }


        #endregion

        #region Property Management

       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveProperty(int id)
        {
            try
            {
                var property = await _context.Properties.FindAsync(id);
                if (property == null)
                {
                    return NotFound();
                }

                property.AdminApproved = true;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Property approved successfully.";
                return RedirectToAction(nameof(ManageProperties));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving property {PropertyId}", id);
                TempData["ErrorMessage"] = "Error approving property. Please try again.";
                return RedirectToAction(nameof(ManageProperties));
            }
        }

        
        #endregion

        #region Category Management

        public async Task<IActionResult> ManageCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .Select(c => new CategoryViewModel
                    {
                        CategoryID = c.CategoryID,
                        CategoryName = c.CategoryName,
                        PropertyCount = c.Properties.Count
                    })
                    .ToListAsync();

                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories");
                TempData["ErrorMessage"] = "Error loading categories. Please try again.";
                return View(new List<CategoryViewModel>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CreateCategoryViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var category = new Category
                    {
                        CategoryName = model.CategoryName
                    };

                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Category created successfully.";
                    return RedirectToAction(nameof(ManageCategories));
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                ModelState.AddModelError("", "Error creating category. Please try again.");
                return View(model);
            }
        }

        #endregion

        #region Review Management

        public async Task<IActionResult> ManageReviews()
        {
            try
            {
                // Initialize ViewBag properties with default values first
                ViewBag.TotalReviews = 0;
                ViewBag.AverageRating = 0.0m;
                ViewBag.PendingDisputes = 0;
                ViewBag.ReviewsThisMonth = 0;
                ViewBag.RatingDistribution = new Dictionary<int, int>
        {
            { 1, 0 },
            { 2, 0 },
            { 3, 0 },
            { 4, 0 },
            { 5, 0 }
        };

                // Get all reviews
                var reviews = await _context.Reviews
                    .Include(r => r.Property)
                    .Include(r => r.Customer)
                    .ToListAsync();

                if (reviews.Any())
                {
                    // Calculate analytics
                    ViewBag.TotalReviews = reviews.Count;
                    ViewBag.AverageRating = reviews.Average(r => r.Rating);
                    ViewBag.PendingDisputes = reviews.Count(r => r.DisputeStatus == DisputeStatus.Disputed);
                    ViewBag.ReviewsThisMonth = reviews.Count(r => r.CreatedDate >= DateTime.Now.AddDays(-30));

                    // Calculate rating distribution
                    ViewBag.RatingDistribution = new Dictionary<int, int>
            {
                { 1, reviews.Count(r => r.Rating == 1) },
                { 2, reviews.Count(r => r.Rating == 2) },
                { 3, reviews.Count(r => r.Rating == 3) },
                { 4, reviews.Count(r => r.Rating == 4) },
                { 5, reviews.Count(r => r.Rating == 5) }
            };
                }

                // Map to view models
                var reviewViewModels = reviews.Select(r => new ReviewManagementViewModel
                {
                    ReviewID = r.ReviewID,
                    PropertyID = r.PropertyID,
                    PropertyNumber = r.Property?.PropertyNumber ?? "N/A",
                    CustomerName = r.Customer != null ?
                        $"{r.Customer.FirstName} {r.Customer.LastName}" : "Anonymous",
                    Rating = r.Rating,
                    ReviewText = r.ReviewText ?? "",
                    HostComments = r.HostComments,
                    DisputeStatus = r.DisputeStatus,
                    DisputeText = r.HostComments, // or wherever you store dispute text
                    CreatedDate = r.CreatedDate
                }).ToList();

                return View(reviewViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reviews for management");
                TempData["ErrorMessage"] = "Error loading reviews. Please try again.";

                // Return view with empty data
                ViewBag.TotalReviews = 0;
                ViewBag.AverageRating = 0.0m;
                ViewBag.PendingDisputes = 0;
                ViewBag.ReviewsThisMonth = 0;
                ViewBag.RatingDistribution = new Dictionary<int, int>
        {
            { 1, 0 },
            { 2, 0 },
            { 3, 0 },
            { 4, 0 },
            { 5, 0 }
        };

                return View(new List<ReviewManagementViewModel>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveDispute(int id, bool upholdReview)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(id);
                if (review == null)
                {
                    return NotFound();
                }

                review.DisputeStatus = upholdReview ? DisputeStatus.Resolved : DisputeStatus.Removed;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Dispute resolved successfully.";
                return RedirectToAction(nameof(ManageReviews));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving review dispute");
                TempData["ErrorMessage"] = "Error resolving dispute. Please try again.";
                return RedirectToAction(nameof(ManageReviews));
            }
        }

       
        #endregion

        #region User Management (Additional Methods)

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                Address = user.Address,
                Birthday = user.Birthday,
                UserType = user.UserType,
                HireStatus = user.HireStatus,
                CurrentRoles = userRoles.ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Phone = model.Phone;
                user.Address = model.Address;
                user.Birthday = model.Birthday;
                user.HireStatus = model.HireStatus;
                user.UserType = model.UserType;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "User updated successfully.";
                    return RedirectToAction(nameof(ManageUsers));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.HireStatus = !user.HireStatus;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"User status {(user.HireStatus ? "activated" : "deactivated")} successfully.";
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpGet]
        public async Task<IActionResult> UserDetails(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var model = new UserDetailsViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                Address = user.Address,
                Birthday = user.Birthday,
                UserType = user.UserType,
                HireStatus = user.HireStatus,
                Roles = roles.ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            string newPassword = "Password123!"; // Default password

            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password reset successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error resetting password.";
            }

            return RedirectToAction(nameof(ManageUsers));
        }


        #region Property Management (Additional Methods)

        [HttpGet]

        [HttpGet]
        public async Task<IActionResult> PropertyDetails(int id)
        {
            try
            {
                var property = await _context.Properties
                    .Include(p => p.Host)
                    .Include(p => p.Category)
                    .Include(p => p.Reviews)
                        .ThenInclude(r => r.Customer)
                    .FirstOrDefaultAsync(p => p.PropertyID == id);

                if (property == null)
                {
                    TempData["ErrorMessage"] = "Property not found.";
                    return RedirectToAction(nameof(ManageProperties));
                }

                var averageRating = property.Reviews.Any()
                    ? property.Reviews.Average(r => r.Rating)
                    : 0;

                var model = new PropertyDetailsViewModel
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
                    AdminApproved = property.AdminApproved,
                    PropertyStatus = property.PropertyStatus,
                    Rating = (decimal)averageRating,
                    ReviewCount = property.Reviews.Count,
                    Reviews = property.Reviews.Select(r => new ReviewViewModel
                    {
                        PropertyNumber = property.PropertyNumber,
                        CustomerName = r.Customer != null ? $"{r.Customer.FirstName} {r.Customer.LastName}" : "Anonymous",
                        Rating = r.Rating,
                        ReviewText = r.ReviewText,
                        CreatedDate = r.CreatedDate
                    }).OrderByDescending(r => r.CreatedDate).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving property details for ID: {PropertyId}", id);
                TempData["ErrorMessage"] = "Error loading property details. Please try again.";
                return RedirectToAction(nameof(ManageProperties));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePropertyStatus(int id)
        {
            try
            {
                var property = await _context.Properties.FindAsync(id);
                if (property == null)
                {
                    return NotFound();
                }

                property.PropertyStatus = !property.PropertyStatus;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Property has been {(property.PropertyStatus ? "activated" : "deactivated")} successfully.";
                return RedirectToAction(nameof(PropertyDetails), new { id = property.PropertyID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling property status");
                TempData["ErrorMessage"] = "Error updating property status. Please try again.";
                return RedirectToAction(nameof(PropertyDetails), new { id });
            }
        }
       

        
       

        #endregion


        [HttpGet]
        public IActionResult RegisterHost()
        {
            return View(new RegisterViewModel { UserType = UserType.Host });
        }

        [HttpPost]
        public IActionResult RegisterCustomer(RegisterViewModel model)
        {
            model.UserType = UserType.Host;
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpGet]
        public IActionResult RegisterCustomer()
        {
            return View(new RegisterViewModel { UserType = UserType.Host });
        }

        [HttpPost]
        public IActionResult RegisterHost(RegisterViewModel model)
        {
            model.UserType = UserType.Host;
            return RedirectToAction(nameof(ManageUsers));
        }

        private async Task<IActionResult> RegisterUser(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new Users
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Phone = model.Phone,
                    Address = model.Address,
                    Birthday = model.Birthday,
                    HireStatus = true,
                    UserType = model.UserType
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.UserType.ToString());
                    TempData["SuccessMessage"] = $"{model.UserType} registered successfully.";
                    return RedirectToAction(nameof(ManageUsers));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]

        public async Task<IActionResult> ManageProperties()
        {
            try
            {
                // First, let's verify if properties exist at all
                var propertiesExist = await _context.Properties.AnyAsync();
                _logger.LogInformation($"Properties exist in database: {propertiesExist}");

                // Get hosts and categories for dropdowns
                ViewBag.Hosts = await _userManager.GetUsersInRoleAsync("Host");
                ViewBag.Categories = await _context.Categories.ToListAsync();

                // Get properties with simpler query first to debug
                var properties = await _context.Properties
                    .Include(p => p.Category)
                    .Include(p => p.Host)
                    .Include(p => p.Reviews)
                    .ToListAsync();

                _logger.LogInformation($"Found {properties.Count} properties");

                // Map to view model
                var propertyViewModels = properties.Select(p => new PropertyManagementDetailViewModel
                {
                    PropertyID = p.PropertyID,
                    PropertyNumber = p.PropertyNumber,
                    HostName = p.Host != null ? $"{p.Host.FirstName} {p.Host.LastName}" : "No Host",
                    Category = p.Category?.CategoryName ?? "Uncategorized",
                    City = p.City,
                    State = p.State,
                    Address = p.Street,
                    ImageUrl = string.IsNullOrEmpty(p.ImageUrl) ? "/images/placeholder.jpg" : p.ImageUrl,
                    IsApproved = p.AdminApproved,
                    IsActive = p.PropertyStatus,
                    WeekdayPrice = p.WeekdayPrice,
                    WeekendPrice = p.WeekendPrice,
                    CleaningFee = p.CleaningFee,
                    DiscountRate = p.DiscountRate,
                    MinNightsForDiscount = p.MinNightsForDiscount,
                    AverageRating = p.Reviews != null && p.Reviews.Any()
                        ? (decimal)p.Reviews.Average(r => r.Rating)
                        : 0,
                    TotalReviews = p.Reviews?.Count ?? 0
                }).ToList();

                _logger.LogInformation($"Mapped {propertyViewModels.Count} property view models");

                return View(propertyViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading properties list");
                TempData["ErrorMessage"] = "Error loading properties. Please try again.";
                return View(new List<PropertyManagementDetailViewModel>());
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectProperty(int id, string reason)
        {
            try
            {
                var property = await _context.Properties
                    .Include(p => p.Host)
                    .FirstOrDefaultAsync(p => p.PropertyID == id);

                if (property == null)
                {
                    return NotFound();
                }

                property.AdminApproved = false;
                property.PropertyStatus = false;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Property rejected successfully.";
                return RedirectToAction(nameof(ManageProperties));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting property {PropertyId}", id);
                TempData["ErrorMessage"] = "Error rejecting property. Please try again.";
                return RedirectToAction(nameof(ManageProperties));
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchProperties(string searchTerm, string status, int? categoryId)
        {
            try
            {
                var query = _context.Properties
                    .Include(p => p.Category)
                    .Include(p => p.Host)
                    .Include(p => p.Reviews)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(p =>
                        p.PropertyNumber.Contains(searchTerm) ||
                        p.City.Contains(searchTerm) ||
                        p.State.Contains(searchTerm) ||
                        p.Host.FirstName.Contains(searchTerm) ||
                        p.Host.LastName.Contains(searchTerm));
                }

                if (!string.IsNullOrEmpty(status))
                {
                    bool isApproved = status.ToLower() == "approved";
                    query = query.Where(p => p.AdminApproved == isApproved);
                }

                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryID == categoryId.Value);
                }

                var properties = await query
                    .Select(p => new PropertyManagementDetailViewModel
                    {
                        // Same mapping as in ManageProperties action
                        // ...
                    })
                    .ToListAsync();

                return PartialView("_PropertyList", properties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching properties");
                return Json(new { error = "Error searching properties" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProperty(AddPropertyViewModel model, IFormFile image)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please check your input and try again.";
                return RedirectToAction(nameof(ManageProperties));
            }

            try
            {
                string imageUrl = "/images/placeholder.jpg";
                if (image != null)
                {
                    // Process and save the image
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "properties");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    Directory.CreateDirectory(uploadsFolder);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    imageUrl = $"/images/properties/{uniqueFileName}";
                }

                var property = new Property
                {
                    PropertyNumber = model.PropertyNumber,
                    HostID = model.HostID,
                    CategoryID = model.CategoryID,
                    Street = model.Street,
                    City = model.City,
                    State = model.State,
                    Zip = model.Zip,
                    WeekdayPrice = model.WeekdayPrice,
                    WeekendPrice = model.WeekendPrice,
                    CleaningFee = model.CleaningFee,
                    DiscountRate = model.DiscountRate,
                    MinNightsForDiscount = model.MinNightsForDiscount,
                    ImageUrl = imageUrl,
                    AdminApproved = false,
                    PropertyStatus = true
                };

                _context.Properties.Add(property);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Property added successfully and pending approval.";
                return RedirectToAction(nameof(ManageProperties));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding property");
                TempData["ErrorMessage"] = "Error adding property. Please try again.";
                return RedirectToAction(nameof(ManageProperties));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Reports(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                if (!startDate.HasValue)
                    startDate = DateTime.Now.AddMonths(-12);
                if (!endDate.HasValue)
                    endDate = DateTime.Now;

                // Get base query for reservations with explicit loading
                var reservations = await _context.Reservations
                    .Include(r => r.Property)
                        .ThenInclude(p => p.Category)
                    .Where(r => r.CreatedDate >= startDate && r.CreatedDate <= endDate)
                    .AsNoTracking()  // For better performance
                    .ToListAsync();

                // Calculate basic metrics
                var totalCommission = reservations.Sum(r => r.Total * 0.10m); // 10% commission
                var totalProperties = await _context.Properties.CountAsync(p => p.AdminApproved);

                // Calculate monthly revenue
                var monthlyRevenue = reservations
                    .GroupBy(r => new { r.CreatedDate.Year, r.CreatedDate.Month })
                    .Select(g => new MonthlyRevenueData
                    {
                        Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Revenue = g.Sum(r => r.Total),
                        Reservations = g.Count()
                    })
                    .OrderBy(m => m.Month)
                    .ToList();

                // Calculate reservations by city
                var reservationsByCity = reservations
                    .GroupBy(r => r.Property.City)
                    .ToDictionary(
                        g => g.Key ?? "Unknown",
                        g => g.Count()
                    );

                // Calculate revenue by category
                var revenueByCategory = reservations
                    .GroupBy(r => r.Property.Category?.CategoryName ?? "Uncategorized")
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(r => r.Total)
                    );

                var viewModel = new AdminReportViewModel
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalReservations = reservations.Count,
                    TotalCommission = totalCommission,
                    TotalProperties = totalProperties,
                    AverageCommissionPerReservation = reservations.Any()
                        ? totalCommission / reservations.Count
                        : 0,
                    MonthlyRevenue = monthlyRevenue,
                    ReservationsByCity = reservationsByCity,
                    RevenueByCategory = revenueByCategory
                };

                _logger.LogInformation($"Generated report with {reservations.Count} reservations");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating admin report");
                TempData["ErrorMessage"] = "Error generating report. Please try again.";
                return View(new AdminReportViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportReport(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var report = await GenerateReportData(startDate, endDate);
                var csv = new StringBuilder();

                // Report Header
                csv.AppendLine("BevoBnB Analytics Report");
                csv.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                csv.AppendLine($"Report Period: {startDate?.ToString("d")} to {endDate?.ToString("d")}");
                csv.AppendLine();

                // Summary Statistics
                csv.AppendLine("Summary Statistics");
                csv.AppendLine("Metric,Value");
                csv.AppendLine($"Total Reservations,{report.TotalReservations}");
                csv.AppendLine($"Total Commission,{report.TotalCommission:C}");
                csv.AppendLine($"Average Commission per Reservation,{report.AverageCommissionPerReservation:C}");
                csv.AppendLine($"Total Properties,{report.TotalProperties}");
                csv.AppendLine();

                // Monthly Revenue
                csv.AppendLine("Monthly Revenue");
                csv.AppendLine("Month,Revenue,Reservations");
                foreach (var month in report.MonthlyRevenue.OrderBy(m => m.Month))
                {
                    csv.AppendLine($"{month.Month},{month.Revenue:C},{month.Reservations}");
                }
                csv.AppendLine();

                // Reservations by City
                csv.AppendLine("Reservations by City");
                csv.AppendLine("City,Number of Reservations");
                foreach (var city in report.ReservationsByCity.OrderByDescending(x => x.Value))
                {
                    csv.AppendLine($"{city.Key},{city.Value}");
                }
                csv.AppendLine();

                // Revenue by Category
                csv.AppendLine("Revenue by Category");
                csv.AppendLine("Category,Revenue");
                foreach (var category in report.RevenueByCategory.OrderByDescending(x => x.Value))
                {
                    csv.AppendLine($"{category.Key},{category.Value:C}");
                }
                csv.AppendLine();

                // Detailed Reservations (Optional but useful)
                if (report.DetailedReservations != null && report.DetailedReservations.Any())
                {
                    csv.AppendLine("Detailed Reservations");
                    csv.AppendLine("Confirmation Number,Property,Check-In,Check-Out,Guests,Total Amount,Status");
                    foreach (var reservation in report.DetailedReservations)
                    {
                        csv.AppendLine($"{reservation.ConfirmationNumber}," +
                                     $"{reservation.PropertyNumber}," +
                                     $"{reservation.CheckIn:d}," +
                                     $"{reservation.CheckOut:d}," +
                                     $"{reservation.NumOfGuests}," +
                                     $"{reservation.Total:C}," +
                                     $"{reservation.Status}");
                    }
                }

                // Generate file
                byte[] bytes = Encoding.UTF8.GetBytes(csv.ToString());
                string fileName = $"BevoBnB_Analytics_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report");
                TempData["ErrorMessage"] = "Error exporting report. Please try again.";
                return RedirectToAction(nameof(Reports));
            }
        }

        private async Task<AdminReportViewModel> GenerateReportData(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-12);
            if (!endDate.HasValue)
                endDate = DateTime.Now;

            var reservations = await _context.Reservations
                .Include(r => r.Property)
                    .ThenInclude(p => p.Category)
                .Where(r => r.CreatedDate >= startDate && r.CreatedDate <= endDate)
                .AsNoTracking()
                .ToListAsync();

            var totalCommission = reservations.Sum(r => r.Total * 0.10m);
            var totalProperties = await _context.Properties.CountAsync(p => p.AdminApproved);

            var monthlyRevenue = reservations
                .GroupBy(r => new { r.CreatedDate.Year, r.CreatedDate.Month })
                .Select(g => new MonthlyRevenueData
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Revenue = g.Sum(r => r.Total),
                    Reservations = g.Count()
                })
                .OrderBy(m => m.Month)
                .ToList();

            var reservationsByCity = reservations
                .GroupBy(r => r.Property.City)
                .ToDictionary(
                    g => g.Key ?? "Unknown",
                    g => g.Count()
                );

            var revenueByCategory = reservations
                .GroupBy(r => r.Property.Category?.CategoryName ?? "Uncategorized")
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(r => r.Total)
                );

            // Add detailed reservations
            var detailedReservations = reservations.Select(r => new ReservationDetailData
            {
                ConfirmationNumber = r.ConfirmationNumber,
                PropertyNumber = r.Property.PropertyNumber,
                CheckIn = r.CheckIn,
                CheckOut = r.CheckOut,
                NumOfGuests = r.NumOfGuests,
                Total = r.Total,
                Status = r.ReservationStatus.ToString()
            }).ToList();

            return new AdminReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalReservations = reservations.Count,
                TotalCommission = totalCommission,
                TotalProperties = totalProperties,
                AverageCommissionPerReservation = reservations.Any() ? totalCommission / reservations.Count : 0,
                MonthlyRevenue = monthlyRevenue,
                ReservationsByCity = reservationsByCity,
                RevenueByCategory = revenueByCategory,
                DetailedReservations = detailedReservations
            };
        }
        [HttpGet]
        public async Task<IActionResult> ManageReservations()
        {
            var reservations = await _context.Reservations
                .Include(r => r.Property)
                .Include(r => r.Customer)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            return View(reservations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmReservation(int reservationId)
        {
            try
            {
                var reservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.ReservationID == reservationId);

                if (reservation == null)
                {
                    TempData["ErrorMessage"] = "Reservation not found.";
                    return RedirectToAction(nameof(ManageReservations));
                }

                reservation.ReservationStatus = ReservationStatus.Confirmed;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Reservation {reservation.ConfirmationNumber} has been confirmed.";
                return RedirectToAction(nameof(ManageReservations));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming reservation");
                TempData["ErrorMessage"] = "An error occurred while confirming the reservation.";
                return RedirectToAction(nameof(ManageReservations));
            }
        }






        #endregion



    }
}