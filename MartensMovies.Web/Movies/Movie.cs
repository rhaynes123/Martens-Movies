namespace MartensMovies.Web.Movies;

public record Movie(
    Guid Id,
    string Title,
    DateOnly ReleaseDate,
    Genre Genre,
    decimal Price,
    int Rating);
