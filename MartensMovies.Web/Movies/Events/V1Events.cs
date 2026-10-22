namespace MartensMovies.Web.Movies.Events.V1;

public record MovieCreated(
    Guid Id,
    string Title,
    DateOnly ReleaseDate,
    Genre Genre,
    decimal Price);

public record MovieEdited(
    string Title,
    DateOnly ReleaseDate,
    Genre Genre,
    decimal Price);

public record MovieDeleted;
