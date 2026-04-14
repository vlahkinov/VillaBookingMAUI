using System.Text.Json.Serialization;

namespace VillaBookingMAUI.Models
{
    public class ApiResponse
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; } = new();

        [JsonPropertyName("result")]
        public object? Result { get; set; }
    }

    /// <summary>
    /// Модел за проверка на наличност на къща.
    /// </summary>
    public class AvailabilityResult
    {
        [JsonPropertyName("houseId")]
        public int HouseId { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("isAvailable")]
        public bool IsAvailable { get; set; }
    }
}
