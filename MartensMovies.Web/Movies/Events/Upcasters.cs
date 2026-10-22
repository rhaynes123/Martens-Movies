using Marten.Services.Json.Transformations;

namespace MartensMovies.Web.Movies.Events;

public class MovieCreatedUpcaster : EventUpcaster<V1.MovieCreated, V2.MovieCreated>
{
    protected override V2.MovieCreated Upcast(V1.MovieCreated old) =>
        new(old.Id, old.Title, old.ReleaseDate, old.Genre, old.Price, Rating: 0);
}

public class MovieEditedUpcaster : EventUpcaster<V1.MovieEdited, V2.MovieEdited>
{
    protected override V2.MovieEdited Upcast(V1.MovieEdited old) =>
        new(old.Title, old.ReleaseDate, old.Genre, old.Price, Rating: 0);
}
