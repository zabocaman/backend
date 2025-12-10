using backend.Services;
using backend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

public class FlightsController : Controller
{
    private readonly FlightApiService _flightApiService;

    public FlightsController(FlightApiService flightApiService)
    {
        _flightApiService = flightApiService;
    }

    [HttpGet]
    public async Task<IActionResult> Arrivals(string? airline, CancellationToken cancellationToken)
    {
        var result = await _flightApiService.GetFlightsAsync("arrivals", cancellationToken);
        return View("Board", BuildViewModel("Arrivals", "arrivals", airline, result));
    }

    [HttpGet]
    public async Task<IActionResult> Departures(string? airline, CancellationToken cancellationToken)
    {
        var result = await _flightApiService.GetFlightsAsync("departures", cancellationToken);
        return View("Board", BuildViewModel("Departures", "departures", airline, result));
    }

    private FlightBoardViewModel BuildViewModel(string title, string direction, string? airlineFilter, FlightApiResult result)
    {
        var flights = result.Flights.ToList();
        var featuredAirline = "Porter Airlines";
        var airlines = flights
            .Select(f => f.Airline)
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(a => a, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var porterIndex = airlines.FindIndex(a => a.Equals(featuredAirline, StringComparison.OrdinalIgnoreCase));
        if (porterIndex >= 0)
        {
            airlines.RemoveAt(porterIndex);
        }

        airlines.Insert(0, featuredAirline);

        var preferredAirline = string.IsNullOrWhiteSpace(airlineFilter) ? "All" : airlineFilter;

        if (!string.IsNullOrWhiteSpace(preferredAirline) && !preferredAirline.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            flights = flights
                .Where(f => f.Airline.Equals(preferredAirline, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Surface Porter Airlines to the top when present
        flights = flights
            .OrderByDescending(f => f.Airline.Equals(featuredAirline, StringComparison.OrdinalIgnoreCase))
            .ThenBy(f => f.Airline)
            .ThenBy(f => f.PlannedTimeDisplay)
            .ToList();

        airlines.Insert(0, "All");

        return new FlightBoardViewModel
        {
            PageTitle = title,
            Direction = direction,
            AirlineFilter = preferredAirline,
            Airlines = airlines,
            Flights = flights,
            AlertMessage = result.Error,
            FeaturedAirline = featuredAirline
        };
    }
}
