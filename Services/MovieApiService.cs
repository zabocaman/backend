using System.Net.Http.Headers;
using System.Text.Json;
using backend.Models;
using backend.ViewModels;
using Microsoft.Extensions.Options;

namespace backend.Services;

public class MovieApiService
{
    private readonly HttpClient _client;
    private readonly RapidApiOptions _options;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public MovieApiService(HttpClient client, IOptions<RapidApiOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<MovieResultViewModel>> SearchMoviesAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Array.Empty<MovieResultViewModel>();
        }

        if (string.IsNullOrWhiteSpace(_options.Key))
        {
            throw new InvalidOperationException("RapidAPI key is missing. Set RapidApi:Key in configuration.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"?s={Uri.EscapeDataString(searchTerm)}&r=json&page=1");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("x-rapidapi-key", _options.Key);

        using var response = await _client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var searchBody = await response.Content.ReadAsStreamAsync(cancellationToken);
        var searchResponse = await JsonSerializer.DeserializeAsync<SearchResponse>(searchBody, _jsonOptions, cancellationToken);

        if (searchResponse?.Search is null || !string.Equals(searchResponse.Response, "True", StringComparison.OrdinalIgnoreCase))
        {
            return Array.Empty<MovieResultViewModel>();
        }

        var results = new List<MovieResultViewModel>();

        foreach (var item in searchResponse.Search)
        {
            var detail = await FetchMovieDetailAsync(item.ImdbId, cancellationToken);
            if (detail is null)
            {
                continue;
            }

            var leadActor = detail.Actors?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? "Unknown";

            results.Add(new MovieResultViewModel
            {
                Title = detail.Title ?? item.Title,
                Year = detail.Year ?? item.Year,
                ImdbId = item.ImdbId,
                PosterUrl = item.Poster,
                Genre = detail.Genre ?? "Unknown",
                LeadActor = leadActor,
                Plot = detail.Plot ?? "Plot details unavailable."
            });
        }

        return results;
    }

    private async Task<MovieDetailResponse?> FetchMovieDetailAsync(string imdbId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(imdbId))
        {
            return null;
        }

        using var detailRequest = new HttpRequestMessage(HttpMethod.Get, $"?i={imdbId}&r=json");
        detailRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        detailRequest.Headers.Add("x-rapidapi-key", _options.Key);

        using var response = await _client.SendAsync(detailRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var detailBody = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<MovieDetailResponse>(detailBody, _jsonOptions, cancellationToken);
    }
}
