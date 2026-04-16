using System.Text.Json.Serialization;

namespace VillaBookingMAUI.Models
{
    public class Booking
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("clientName")]
        public string ClientName { get; set; } = string.Empty;

        [JsonPropertyName("guestsCount")]
        public int GuestsCount { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("houseId")]
        public int HouseId { get; set; }

        [JsonPropertyName("isDepositPaid")]
        public bool IsDepositPaid { get; set; }

        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; } = string.Empty;

        [JsonPropertyName("clientPhone")]
        public string? ClientPhone { get; set; }

        [JsonPropertyName("clientEmail")]
        public string? ClientEmail { get; set; }

        // Computed properties for UI display
        public string HouseDisplayName => HouseId == 1 ? "Къща 1 – Планинска" : "Къща 2 – Езерна";
        public string DateRange => $"{StartDate:dd MMM yyyy} – {EndDate:dd MMM yyyy}";
        public int Nights => (EndDate - StartDate).Days;
        public string GuestsDisplay => GuestsCount == 1 ? "1 гост" : $"{GuestsCount} гости";
        public string DepositStatus => IsDepositPaid ? "Платен" : "Неплатен";
        public Color DepositColor => IsDepositPaid ? Color.FromArgb("#2ECC71") : Color.FromArgb("#E74C3C");
        public bool HasPhone => !string.IsNullOrWhiteSpace(ClientPhone);
        public bool HasEmail => !string.IsNullOrWhiteSpace(ClientEmail);
        public bool HasContact => HasPhone || HasEmail;
    }
}
