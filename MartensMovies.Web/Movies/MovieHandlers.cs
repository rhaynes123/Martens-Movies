using MartensMovies.Web.Movies.Events.V2;
using V1 = MartensMovies.Web.Movies.Events.V1;
using Marten;
using Wolverine.Marten;

namespace MartensMovies.Web.Movies;

public static class CreateMovieHandler
{
    public static Guid Handle(CreateMovie cmd, IDocumentSession session)
    {
        var id = Guid.NewGuid();
        var created = new MovieCreated(id, cmd.Title, cmd.ReleaseDate, cmd.Genre, cmd.Price, cmd.Rating);
        session.Events.StartStream<Movie>(id, created);
        return id;
    }
}

public static class EditMovieHandler
{
    [AggregateHandler]
    public static MovieEdited Handle(EditMovie cmd, Movie _) =>
        new(cmd.Title, cmd.ReleaseDate, cmd.Genre, cmd.Price, cmd.Rating);
}

public static class DeleteMovieHandler
{
    [AggregateHandler]
    public static V1.MovieDeleted Handle(DeleteMovie cmd, Movie _) => new();
}
