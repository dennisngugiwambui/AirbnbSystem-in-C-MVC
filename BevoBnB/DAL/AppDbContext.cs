using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BevoBnB.Models;
using Mono.TextTemplating;
using State = BevoBnB.Models.State;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BevoBnB.DAL
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions options) : base(options) 
        {
            Database.SetCommandTimeout(TimeSpan.FromSeconds(30));
        }
        public DbSet<Property> Properties { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<PropertyUnavailability> PropertyUnavailabilities { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageReply> MessageReplies { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
            optionsBuilder.EnableSensitiveDataLogging(false);
            optionsBuilder.EnableDetailedErrors(true);
            base.OnConfiguring(optionsBuilder);
        }

      

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite key for PropertyUnavailability
            modelBuilder.Entity<PropertyUnavailability>()
                .HasKey(pu => new { pu.PropertyID, pu.UnavailableDate });

            // Configure starting IDs
            modelBuilder.Entity<Property>()
                .Property(p => p.PropertyID)
                .UseIdentityColumn(3001, 1);

            modelBuilder.Entity<Reservation>()
                .Property(r => r.ReservationID)
                .UseIdentityColumn(21901, 1);

            // Configure relationships
            modelBuilder.Entity<Property>()
                .HasOne(p => p.Host)
                .WithMany(u => u.Properties)
                .HasForeignKey(p => p.HostID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Customer)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.CustomerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.CustomerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MessageReply>()
                .HasOne(r => r.Sender)
                .WithMany()
                .HasForeignKey(r => r.SenderID)
                .OnDelete(DeleteBehavior.Restrict);

            var hasher = new PasswordHasher<Users>();

            // Seed Roles
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "2", Name = "Host", NormalizedName = "HOST" },
                new IdentityRole { Id = "3", Name = "Customer", NormalizedName = "CUSTOMER" }
            );

            // Seed Admin Users
            var admin1 = new Users
            {
                Id = "1",
                UserName = "admin1@bevobnb.com",
                NormalizedUserName = "ADMIN1@BEVOBNB.COM",
                Email = "admin1@bevobnb.com",
                NormalizedEmail = "ADMIN1@BEVOBNB.COM",
                FirstName = "System",
                LastName = "Admin",
                Birthday = new DateTime(1990, 1, 1),
                Address = "123 Admin St",
                Phone = "555-0123",
                UserType = UserType.Admin,
                HireStatus = true,
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, "Admin123!"),
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var admin2 = new Users
            {
                Id = "2",
                UserName = "admin2@bevobnb.com",
                NormalizedUserName = "ADMIN2@BEVOBNB.COM",
                Email = "admin2@bevobnb.com",
                NormalizedEmail = "ADMIN2@BEVOBNB.COM",
                FirstName = "Main",
                LastName = "Admin",
                Birthday = new DateTime(1992, 2, 2),
                Address = "456 Admin Ave",
                Phone = "555-0124",
                UserType = UserType.Admin,
                HireStatus = true,
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, "Admin123!"),
                SecurityStamp = Guid.NewGuid().ToString()
            };

            // Seed Host User
            var host = new Users
            {
                Id = "3",
                UserName = "host@bevobnb.com",
                NormalizedUserName = "HOST@BEVOBNB.COM",
                Email = "host@bevobnb.com",
                NormalizedEmail = "HOST@BEVOBNB.COM",
                FirstName = "Test",
                LastName = "Host",
                Birthday = new DateTime(1985, 5, 15),
                Address = "789 Host Rd",
                Phone = "555-0125",
                UserType = UserType.Host,
                HireStatus = true,
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, "Host123!"),
                SecurityStamp = Guid.NewGuid().ToString()
            };

            // Seed Customer User
            var customer = new Users
            {
                Id = "4",
                UserName = "customer@bevobnb.com",
                NormalizedUserName = "CUSTOMER@BEVOBNB.COM",
                Email = "customer@bevobnb.com",
                NormalizedEmail = "CUSTOMER@BEVOBNB.COM",
                FirstName = "Test",
                LastName = "Customer",
                Birthday = new DateTime(1995, 8, 20),
                Address = "101 Customer Ln",
                Phone = "555-0126",
                UserType = UserType.Customer,
                HireStatus = true,
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, "Customer123!"),
                SecurityStamp = Guid.NewGuid().ToString()
            };

            // Add users to their roles
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string> { UserId = admin1.Id, RoleId = "1" },
                new IdentityUserRole<string> { UserId = admin2.Id, RoleId = "1" },
                new IdentityUserRole<string> { UserId = host.Id, RoleId = "2" },
                new IdentityUserRole<string> { UserId = customer.Id, RoleId = "3" }
            );

            // Add users
            modelBuilder.Entity<Users>().HasData(admin1, admin2, host, customer);

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryID = 1, CategoryName = "House" },
                new Category { CategoryID = 2, CategoryName = "Apartment" },
                new Category { CategoryID = 3, CategoryName = "Condo" },
                new Category { CategoryID = 4, CategoryName = "Cabin" },
                new Category { CategoryID = 5, CategoryName = "Villa" },
                new Category { CategoryID = 6, CategoryName = "Hotel Room" },
                new Category { CategoryID = 7, CategoryName = "Guest House" },
                new Category { CategoryID = 8, CategoryName = "Tiny House" },
                new Category { CategoryID = 9, CategoryName = "Beach House" },
                new Category { CategoryID = 10, CategoryName = "Farmhouse" }
            );

            // Seed US States
            modelBuilder.Entity<State>().HasData(
                new State { StateID = 1, StateCode = "AL", StateName = "Alabama" },
                new State { StateID = 2, StateCode = "AK", StateName = "Alaska" },
                new State { StateID = 3, StateCode = "AZ", StateName = "Arizona" },
                new State { StateID = 4, StateCode = "AR", StateName = "Arkansas" },
                new State { StateID = 5, StateCode = "CA", StateName = "California" },
                new State { StateID = 6, StateCode = "CO", StateName = "Colorado" },
                new State { StateID = 7, StateCode = "CT", StateName = "Connecticut" },
                new State { StateID = 8, StateCode = "DE", StateName = "Delaware" },
                new State { StateID = 9, StateCode = "FL", StateName = "Florida" },
                new State { StateID = 10, StateCode = "GA", StateName = "Georgia" },
                new State { StateID = 11, StateCode = "HI", StateName = "Hawaii" },
                new State { StateID = 12, StateCode = "ID", StateName = "Idaho" },
                new State { StateID = 13, StateCode = "IL", StateName = "Illinois" },
                new State { StateID = 14, StateCode = "IN", StateName = "Indiana" },
                new State { StateID = 15, StateCode = "IA", StateName = "Iowa" },
                new State { StateID = 16, StateCode = "KS", StateName = "Kansas" },
                new State { StateID = 17, StateCode = "KY", StateName = "Kentucky" },
                new State { StateID = 18, StateCode = "LA", StateName = "Louisiana" },
                new State { StateID = 19, StateCode = "ME", StateName = "Maine" },
                new State { StateID = 20, StateCode = "MD", StateName = "Maryland" },
                new State { StateID = 21, StateCode = "MA", StateName = "Massachusetts" },
                new State { StateID = 22, StateCode = "MI", StateName = "Michigan" },
                new State { StateID = 23, StateCode = "MN", StateName = "Minnesota" },
                new State { StateID = 24, StateCode = "MS", StateName = "Mississippi" },
                new State { StateID = 25, StateCode = "MO", StateName = "Missouri" },
                new State { StateID = 26, StateCode = "MT", StateName = "Montana" },
                new State { StateID = 27, StateCode = "NE", StateName = "Nebraska" },
                new State { StateID = 28, StateCode = "NV", StateName = "Nevada" },
                new State { StateID = 29, StateCode = "NH", StateName = "New Hampshire" },
                new State { StateID = 30, StateCode = "NJ", StateName = "New Jersey" },
                new State { StateID = 31, StateCode = "NM", StateName = "New Mexico" },
                new State { StateID = 32, StateCode = "NY", StateName = "New York" },
                new State { StateID = 33, StateCode = "NC", StateName = "North Carolina" },
                new State { StateID = 34, StateCode = "ND", StateName = "North Dakota" },
                new State { StateID = 35, StateCode = "OH", StateName = "Ohio" },
                new State { StateID = 36, StateCode = "OK", StateName = "Oklahoma" },
                new State { StateID = 37, StateCode = "OR", StateName = "Oregon" },
                new State { StateID = 38, StateCode = "PA", StateName = "Pennsylvania" },
                new State { StateID = 39, StateCode = "RI", StateName = "Rhode Island" },
                new State { StateID = 40, StateCode = "SC", StateName = "South Carolina" },
                new State { StateID = 41, StateCode = "SD", StateName = "South Dakota" },
                new State { StateID = 42, StateCode = "TN", StateName = "Tennessee" },
                new State { StateID = 43, StateCode = "TX", StateName = "Texas" },
                new State { StateID = 44, StateCode = "UT", StateName = "Utah" },
                new State { StateID = 45, StateCode = "VT", StateName = "Vermont" },
                new State { StateID = 46, StateCode = "VA", StateName = "Virginia" },
                new State { StateID = 47, StateCode = "WA", StateName = "Washington" },
                new State { StateID = 48, StateCode = "WV", StateName = "West Virginia" },
                new State { StateID = 49, StateCode = "WI", StateName = "Wisconsin" },
                new State { StateID = 50, StateCode = "WY", StateName = "Wyoming" }
            );
        }
    }
}