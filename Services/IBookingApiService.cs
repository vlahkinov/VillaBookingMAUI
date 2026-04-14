using VillaBookingMAUI.Models;

namespace VillaBookingMAUI.Services
{
    public interface IBookingApiService
    {
        Task<List<Booking>> GetAllBookingsAsync();
        Task<Booking?> GetBookingByIdAsync(int id);
        Task<(bool Success, string? Error)> CreateBookingAsync(Booking booking);
        Task<(bool Success, string? Error)> UpdateBookingAsync(Booking booking);
        Task<(bool Success, string? Error)> DeleteBookingAsync(int id);
        Task<List<Booking>> GetBookingsByHouseAsync(int houseId, int? month = null, int? year = null);
        Task<bool> CheckAvailabilityAsync(int houseId, DateTime startDate, DateTime endDate);
    }
}
