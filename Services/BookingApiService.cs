using System.Text;
using System.Text.Json;
using VillaBookingMAUI.Models;

namespace VillaBookingMAUI.Services
{
    public class BookingApiService : IBookingApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public BookingApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Помощен метод: извлича Result от ApiResponse като суров JSON string.
        /// System.Text.Json десериализира object? като JsonElement, затова
        /// трябва да използваме GetRawText() вместо ToString().
        /// </summary>
        private string? ExtractResultJson(ApiResponse? apiResponse)
        {
            if (apiResponse?.IsSuccess != true || apiResponse.Result == null)
                return null;

            if (apiResponse.Result is JsonElement jsonElement)
                return jsonElement.GetRawText();

            // Fallback ако Result е вече десериализиран обект
            return JsonSerializer.Serialize(apiResponse.Result, _jsonOptions);
        }

        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/bookings");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[API] GetAll response: {content}");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(content, _jsonOptions);
                var resultJson = ExtractResultJson(apiResponse);

                if (resultJson != null)
                {
                    var bookings = JsonSerializer.Deserialize<List<Booking>>(resultJson, _jsonOptions);
                    System.Diagnostics.Debug.WriteLine($"[API] Deserialized {bookings?.Count ?? 0} bookings");
                    return bookings ?? new();
                }

                return new();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] GetAllBookingsAsync error: {ex.Message}");
                return new();
            }
        }

        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/bookings/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[API] GetById response: {content}");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(content, _jsonOptions);
                var resultJson = ExtractResultJson(apiResponse);

                if (resultJson != null)
                {
                    return JsonSerializer.Deserialize<Booking>(resultJson, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] GetBookingByIdAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<(bool Success, string? Error)> CreateBookingAsync(Booking booking)
        {
            try
            {
                var json = JsonSerializer.Serialize(booking, _jsonOptions);
                System.Diagnostics.Debug.WriteLine($"[API] Create request: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/bookings", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[API] Create response ({response.StatusCode}): {responseContent}");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent, _jsonOptions);

                if (response.IsSuccessStatusCode && apiResponse?.IsSuccess == true)
                    return (true, null);

                var error = apiResponse?.Errors?.FirstOrDefault() ?? "Неизвестна грешка при създаване.";
                return (false, error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] CreateBookingAsync error: {ex.Message}");
                return (false, $"Грешка при връзка със сървъра: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? Error)> UpdateBookingAsync(Booking booking)
        {
            try
            {
                var json = JsonSerializer.Serialize(booking, _jsonOptions);
                System.Diagnostics.Debug.WriteLine($"[API] Update request: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/bookings/{booking.Id}", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[API] Update response ({response.StatusCode}): {responseContent}");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent, _jsonOptions);

                if (response.IsSuccessStatusCode && apiResponse?.IsSuccess == true)
                    return (true, null);

                var error = apiResponse?.Errors?.FirstOrDefault() ?? "Неизвестна грешка при обновяване.";
                return (false, error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] UpdateBookingAsync error: {ex.Message}");
                return (false, $"Грешка при връзка със сървъра: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? Error)> DeleteBookingAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/bookings/{id}");
                var responseContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[API] Delete response ({response.StatusCode}): {responseContent}");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent, _jsonOptions);

                if (response.IsSuccessStatusCode && apiResponse?.IsSuccess == true)
                    return (true, null);

                var error = apiResponse?.Errors?.FirstOrDefault() ?? "Неизвестна грешка при изтриване.";
                return (false, error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] DeleteBookingAsync error: {ex.Message}");
                return (false, $"Грешка при връзка със сървъра: {ex.Message}");
            }
        }

        public async Task<List<Booking>> GetBookingsByHouseAsync(int houseId, int? month = null, int? year = null)
        {
            try
            {
                var url = $"api/houses/{houseId}/bookings";
                if (month.HasValue && year.HasValue)
                    url += $"?month={month}&year={year}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[API] GetByHouse response: {content}");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(content, _jsonOptions);
                var resultJson = ExtractResultJson(apiResponse);

                if (resultJson != null)
                {
                    return JsonSerializer.Deserialize<List<Booking>>(resultJson, _jsonOptions) ?? new();
                }

                return new();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] GetBookingsByHouseAsync error: {ex.Message}");
                return new();
            }
        }

        public async Task<bool> CheckAvailabilityAsync(int houseId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var url = $"api/houses/{houseId}/availability" +
                          $"?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[API] Availability response: {content}");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(content, _jsonOptions);
                var resultJson = ExtractResultJson(apiResponse);

                if (resultJson != null)
                {
                    var availability = JsonSerializer.Deserialize<AvailabilityResult>(resultJson, _jsonOptions);
                    return availability?.IsAvailable ?? false;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] CheckAvailability error: {ex.Message}");
                return false;
            }
        }
    }
}