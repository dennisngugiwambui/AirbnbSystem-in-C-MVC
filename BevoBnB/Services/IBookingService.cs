using BevoBnB.Models;
using System;

namespace BevoBnB.Services
{
    public interface IBookingService
    {
        bool AreDatesAvailable(int propertyId, DateTime checkIn, DateTime checkOut);
        Task<bool> CreateBooking(int propertyId, DateTime checkIn, DateTime checkOut, int numberOfGuests, string userId);
        Task<IEnumerable<Reservation>> GetUserBookings(string userId);
        Task<bool> CancelBooking(int bookingId, string userId);
    }
}