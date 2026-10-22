namespace MartensMovies.Web.Movies.Events.V2;

public record MovieCreated(
    Guid Id,
    string Title,
    DateOnly ReleaseDate,
    Genre Genre,
    decimal Price,
    int Rating);

public record MovieEdited(
    string Title,
    DateOnly ReleaseDate,
    Genre Genre,
    decimal Price,
    int Rating);
