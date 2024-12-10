using BevoBnB.DAL;
using BevoBnB.Data;
using BevoBnB.Models;
using BevoBnB.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BevoBnB.Services
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        public bool AreDatesAvailable(int propertyId, DateTime checkIn, DateTime checkOut)
        {
            var overlappingBookings = _context.Reservations
                .Where(b => b.PropertyID == propertyId &&
                           b.ReservationStatus != ReservationStatus.Cancelled &&
                           !((b.CheckOut <= checkIn) || (b.CheckIn >= checkOut)))
                .Any();

            return !overlappingBookings;
        }

        public async Task<bool> CreateBooking(int propertyId, DateTime checkIn, DateTime checkOut, int numOfGuests, string userId)
        {
            try
            {
                var property = await _context.Properties.FindAsync(propertyId);
                if (property == null) return false;

                var booking = new Reservation
                {
                    PropertyID = propertyId,
                    CustomerID = userId,
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    NumOfGuests = numOfGuests,
                    ReservationStatus = ReservationStatus.Pending,
                    CreatedDate = DateTime.Now
                };

                _context.Reservations.Add(booking);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Reservation>> GetUserBookings(string userId)
        {
            return await _context.Reservations
                .Include(b => b.Property)
                .Where(b => b.CustomerID == userId)
                .OrderByDescending(b => b.CheckIn)
                .ToListAsync();
        }

        public async Task<bool> CancelBooking(int bookingId, string userId)
        {
            var booking = await _context.Reservations
                .FirstOrDefaultAsync(b => b.ReservationID == bookingId && b.CustomerID == userId);

            if (booking == null) return false;

            booking.ReservationStatus = ReservationStatus.Cancelled;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}