namespace backend.ViewModels;

public class MovieSearchViewModel
{
    public string Query { get; set; } = string.Empty;
    public List<MovieResultViewModel> Results { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public bool HasResults => Results.Count > 0;
}
