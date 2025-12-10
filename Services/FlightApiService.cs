using System.Net.Http.Headers;
using System.Text.Json;
using backend.Models;
using Microsoft.Extensions.Options;

namespace backend.Services;

public class FlightApiService
{
    private readonly HttpClient _client;
    private readonly FlightApiOptions _options;

    public FlightApiService(HttpClient client, IOptions<FlightApiOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task<FlightApiResult> GetFlightsAsync(string direction, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(direction))
        {
            throw new ArgumentException("Direction must be provided", nameof(direction));
        }

        if (string.IsNullOrWhiteSpace(_options.BaseUrl) || string.IsNullOrWhiteSpace(_options.Key))
        {
            return FlightApiResult.FromFallback(direction, "Flight API configuration is missing. Showing sample Pearson data instead.");
        }

        var path = string.IsNullOrWhiteSpace(_options.Path)
            ? $"flights?airport=YYZ&direction={direction}"
            : $"{_options.Path.TrimStart('/')}?airport=YYZ&direction={direction}";

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("x-rapidapi-key", _options.Key);

        if (!string.IsNullOrWhiteSpace(_options.Host))
        {
            request.Headers.Add("x-rapidapi-host", _options.Host);
        }

        try
        {
            using var response = await _client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);

            var flights = ParseFlights(document.RootElement, direction);
            return new FlightApiResult(flights, null);
        }
        catch (Exception ex)
        {
            return FlightApiResult.FromFallback(direction, $"Live flight data unavailable: {ex.Message}");
        }
    }

    private List<FlightInfo> ParseFlights(JsonElement root, string direction)
    {
        var items = new List<JsonElement>();

        if (root.ValueKind == JsonValueKind.Array)
        {
            items.AddRange(root.EnumerateArray());
        }
        else if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
        {
            items.AddRange(data.EnumerateArray());
        }
        else if (root.TryGetProperty(direction, out var directionNode) && directionNode.ValueKind == JsonValueKind.Array)
        {
            items.AddRange(directionNode.EnumerateArray());
        }

        if (items.Count == 0)
        {
            return BuildFallback(direction);
        }

        var flights = new List<FlightInfo>();

        foreach (var item in items)
        {
            var airline = GetString(item, "airline")
                          ?? GetString(item, "airlineName")
                          ?? "Unknown airline";

            var flightNumber = GetString(item, "flightNumber")
                               ?? GetString(item, "flight")
                               ?? string.Empty;

            var destination = GetString(item, "destination")
                              ?? GetString(item, "arrival")
                              ?? GetString(item, "arrives")
                              ?? string.Empty;

            var origin = GetString(item, "origin")
                         ?? GetString(item, "departure")
                         ?? GetString(item, "departs")
                         ?? string.Empty;

            var scheduled = GetString(item, "scheduledTime")
                            ?? GetString(item, "scheduled")
                            ?? GetString(item, "time");

            var gate = GetString(item, "gate") ?? string.Empty;
            var status = GetString(item, "status") ?? string.Empty;
            var terminal = GetString(item, "terminal") ?? string.Empty;

            flights.Add(new FlightInfo
            {
                Airline = airline,
                FlightNumber = flightNumber,
                Destination = destination,
                Origin = origin,
                ScheduledTimeRaw = scheduled,
                Gate = gate,
                Status = status,
                Terminal = terminal,
                Direction = direction
            });
        }

        return flights;
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }

        return null;
    }

    internal static List<FlightInfo> BuildFallback(string direction)
    {
        var isArrival = direction.Equals("arrivals", StringComparison.OrdinalIgnoreCase);
        return new List<FlightInfo>
        {
            new()
            {
                Airline = "Porter Airlines",
                FlightNumber = isArrival ? "PD 123" : "PD 456",
                Origin = isArrival ? "Ottawa" : "Toronto",
                Destination = isArrival ? "Toronto" : "Ottawa",
                ScheduledTimeRaw = "2024-06-30T14:15:00",
                Gate = "B12",
                Status = "On Time",
                Terminal = "T1",
                Direction = direction
            },
            new()
            {
                Airline = "Air Canada",
                FlightNumber = isArrival ? "AC 789" : "AC 101",
                Origin = isArrival ? "Vancouver" : "Toronto",
                Destination = isArrival ? "Toronto" : "Vancouver",
                ScheduledTimeRaw = "2024-06-30T15:05:00",
                Gate = "D7",
                Status = "Delayed",
                Terminal = "T1",
                Direction = direction
            },
            new()
            {
                Airline = "WestJet",
                FlightNumber = isArrival ? "WS 222" : "WS 333",
                Origin = isArrival ? "Calgary" : "Toronto",
                Destination = isArrival ? "Toronto" : "Calgary",
                ScheduledTimeRaw = "2024-06-30T16:40:00",
                Gate = "A2",
                Status = "Boarding",
                Terminal = "T3",
                Direction = direction
            }
        };
    }
}

public record FlightApiResult(IReadOnlyList<FlightInfo> Flights, string? Error)
{
    public static FlightApiResult FromFallback(string direction, string message) =>
        new(FlightApiService.BuildFallback(direction), message);
}
