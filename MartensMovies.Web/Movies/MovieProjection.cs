using MartensMovies.Web.Movies.Events.V1;
using MartensMovies.Web.Movies.Events.V2;
using Marten.Events.Aggregation;

namespace MartensMovies.Web.Movies;

public class MovieProjection : SingleStreamProjection<Movie, Guid>
{
    public MovieProjection()
    {
        DeleteEvent<MovieDeleted>();
    }

    public static Movie Create(Events.V2.MovieCreated e) =>
        new(e.Id, e.Title, e.ReleaseDate, e.Genre, e.Price, e.Rating);

    public static Movie Apply(Events.V2.MovieEdited e, Movie current) =>
        current with
        {
            Title = e.Title,
            ReleaseDate = e.ReleaseDate,
            Genre = e.Genre,
            Price = e.Price,
            Rating = e.Rating
        };
}
