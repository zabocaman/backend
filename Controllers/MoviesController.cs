using backend.Services;
using backend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

public class MoviesController : Controller
{
    private readonly MovieApiService _movieApiService;

    public MoviesController(MovieApiService movieApiService)
    {
        _movieApiService = movieApiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, CancellationToken cancellationToken)
    {
        var viewModel = new MovieSearchViewModel
        {
            Query = searchTerm ?? string.Empty
        };

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            try
            {
                var results = await _movieApiService.SearchMoviesAsync(searchTerm, cancellationToken);
                if (results.Any())
                {
                    viewModel.Results = results.ToList();
                }
                else
                {
                    viewModel.ErrorMessage = "No movies found for that search. Try another title or broaden your keywords.";
                }
            }
            catch (Exception ex)
            {
                viewModel.ErrorMessage = $"We hit a snag while talking to the movie database: {ex.Message}";
            }
        }

        return View(viewModel);
    }
}
