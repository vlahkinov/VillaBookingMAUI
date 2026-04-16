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

        private static string BuildRequestError(HttpResponseMessage response, ApiResponse? apiResponse, string fallbackMessage)
        {
            var apiError = apiResponse?.Errors?.FirstOrDefault();
            return apiError ?? $"{fallbackMessage} (HTTP {(int)response.StatusCode})";
        }

        private async Task<ApiResponse?> ReadApiResponseAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ApiResponse>(content, _jsonOptions);
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
            var response = await _httpClient.GetAsync("api/bookings");
            var apiResponse = await ReadApiResponseAsync(response);

            if (!response.IsSuccessStatusCode || apiResponse?.IsSuccess != true)
                throw new HttpRequestException(BuildRequestError(response, apiResponse, "Failed to load bookings."));

            var resultJson = ExtractResultJson(apiResponse);
            if (resultJson == null)
                return new();

            var bookings = JsonSerializer.Deserialize<List<Booking>>(resultJson, _jsonOptions);
            System.Diagnostics.Debug.WriteLine($"[API] Deserialized {bookings?.Count ?? 0} bookings");
            return bookings ?? new();
        }

        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/bookings/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                var apiResponse = await ReadApiResponseAsync(response);
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
                var apiResponse = await ReadApiResponseAsync(response);

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
                var apiResponse = await ReadApiResponseAsync(response);

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
                var apiResponse = await ReadApiResponseAsync(response);

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
            var url = $"api/houses/{houseId}/bookings";
            if (month.HasValue && year.HasValue)
                url += $"?month={month}&year={year}";

            var response = await _httpClient.GetAsync(url);
            var apiResponse = await ReadApiResponseAsync(response);

            if (!response.IsSuccessStatusCode || apiResponse?.IsSuccess != true)
                throw new HttpRequestException(BuildRequestError(response, apiResponse, "Failed to load house bookings."));

            var resultJson = ExtractResultJson(apiResponse);
            if (resultJson != null)
            {
                return JsonSerializer.Deserialize<List<Booking>>(resultJson, _jsonOptions) ?? new();
            }

            return new();
        }

        public async Task<(bool IsAvailable, string? Error)> CheckAvailabilityAsync(
            int houseId,
            DateTime startDate,
            DateTime endDate,
            int? excludeBookingId = null)
        {
            try
            {
                var url = $"api/houses/{houseId}/availability" +
                          $"?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
                if (excludeBookingId.HasValue)
                    url += $"&excludeBookingId={excludeBookingId.Value}";

                var response = await _httpClient.GetAsync(url);
                var apiResponse = await ReadApiResponseAsync(response);

                if (!response.IsSuccessStatusCode || apiResponse?.IsSuccess != true)
                    return (false, BuildRequestError(response, apiResponse, "Failed to check availability."));

                var resultJson = ExtractResultJson(apiResponse);

                if (resultJson != null)
                {
                    var availability = JsonSerializer.Deserialize<AvailabilityResult>(resultJson, _jsonOptions);
                    return (availability?.IsAvailable ?? false, null);
                }

                return (false, "The server returned an empty availability response.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] CheckAvailability error: {ex.Message}");
                return (false, $"Грешка при връзка със сървъра: {ex.Message}");
            }
        }
    }
}
