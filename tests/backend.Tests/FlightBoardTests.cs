using backend.Controllers;
using backend.Models;
using backend.Services;

namespace backend.Tests;

public class FlightBoardTests
{
    private static readonly FlightInfo[] Flights =
    {
        new()
        {
            Airline = "Air Canada",
            FlightNumber = "AC 101",
            Origin = "Toronto",
            Destination = "Vancouver",
            ScheduledTimeRaw = "2026-09-18T15:05:00",
            Direction = "departures"
        },
        new()
        {
            Airline = "Porter Airlines",
            FlightNumber = "PD 456",
            Origin = "Toronto",
            Destination = "Ottawa",
            ScheduledTimeRaw = "2026-09-18T14:15:00",
            Direction = "departures"
        }
    };

    [Fact]
    public void DefaultFilterShowsAllAndPinsPorter()
    {
        var model = FlightsController.BuildViewModel(
            "Departures",
            "departures",
            null,
            new FlightApiResult(Flights, null));

        Assert.Equal("All", model.AirlineFilter);
        Assert.Equal(new[] { "All", "Porter Airlines", "Air Canada" }, model.Airlines);
        Assert.Equal(new[] { "Porter Airlines", "Air Canada" }, model.Flights.Select(f => f.Airline));
    }

    [Fact]
    public void AirlineFilterIsTrimmedAndAppliedCaseInsensitively()
    {
        var model = FlightsController.BuildViewModel(
            "Departures",
            "departures",
            " air canada ",
            new FlightApiResult(Flights, null));

        var flight = Assert.Single(model.Flights);
        Assert.Equal("Air Canada", flight.Airline);
        Assert.Equal("air canada", model.AirlineFilter);
    }

    [Theory]
    [InlineData("arrivals", "Montreal", "Toronto", "Montreal")]
    [InlineData("departures", "Toronto", "Montreal", "Montreal")]
    public void RouteDisplayUsesDirection(
        string direction,
        string origin,
        string destination,
        string expected)
    {
        var flight = new FlightInfo
        {
            Direction = direction,
            Origin = origin,
            Destination = destination
        };

        Assert.Equal(expected, flight.RouteDisplay);
    }
}
