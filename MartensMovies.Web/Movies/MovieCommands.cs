using System.ComponentModel.DataAnnotations;

namespace MartensMovies.Web.Movies;

public record CreateMovie(
    [property: Required, StringLength(100, MinimumLength = 1)] string Title,
    [property: Required] DateOnly ReleaseDate,
    [property: Required] Genre Genre,
    [property: Range(0, 1000)] decimal Price,
    [property: Range(0, 10)] int Rating);

public record EditMovie(
    Guid MovieId,
    [property: Required, StringLength(100, MinimumLength = 1)] string Title,
    [property: Required] DateOnly ReleaseDate,
    [property: Required] Genre Genre,
    [property: Range(0, 1000)] decimal Price,
    [property: Range(0, 10)] int Rating);

public record DeleteMovie(Guid MovieId);
