using System.Globalization;
using System.Text.Json.Serialization;

namespace backend.Models;

public class FlightInfo
{
    [JsonPropertyName("airline")]
    public string Airline { get; set; } = string.Empty;

    [JsonPropertyName("flightNumber")]
    public string FlightNumber { get; set; } = string.Empty;

    [JsonPropertyName("destination")]
    public string Destination { get; set; } = string.Empty;

    [JsonPropertyName("origin")]
    public string Origin { get; set; } = string.Empty;

    [JsonPropertyName("scheduledTime")]
    public string? ScheduledTimeRaw { get; set; }

    public string Gate { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Terminal { get; set; } = string.Empty;

    public string Direction { get; set; } = string.Empty;

    public string PlannedTimeDisplay =>
        DateTime.TryParse(ScheduledTimeRaw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed)
            ? parsed.ToString("HH:mm")
            : (ScheduledTimeRaw ?? "TBD");

    public string RouteDisplay =>
        Direction.Equals("arrivals", StringComparison.OrdinalIgnoreCase)
            ? (!string.IsNullOrWhiteSpace(Origin) ? Origin : Destination)
            : (!string.IsNullOrWhiteSpace(Destination) ? Destination : Origin);
}
