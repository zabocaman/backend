using backend.Models;

namespace backend.ViewModels;

public class FlightBoardViewModel
{
    public string PageTitle { get; init; } = string.Empty;
    public string Direction { get; init; } = string.Empty;
    public string AirlineFilter { get; init; } = "All";
    public IReadOnlyList<string> Airlines { get; init; } = Array.Empty<string>();
    public IReadOnlyList<FlightInfo> Flights { get; init; } = Array.Empty<FlightInfo>();
    public string FeaturedAirline { get; init; } = "Porter Airlines";
    public string? AlertMessage { get; init; }
    public bool HasFlights => Flights.Count > 0;
}
