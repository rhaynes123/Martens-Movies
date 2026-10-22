using Bobcat.Xunit;
using MartensMovies.Web.Movies;
using MartensMovies.Web.Movies.Events;
using MartensMovies.Web.Movies.Events.V2;
using V1 = MartensMovies.Web.Movies.Events.V1;

namespace MartensMovies.Tests;

public class MovieHandlerTests
{
    [BobcatSpec]
    public void EditMovieHandler_returns_MovieEdited_from_command()
    {
        var current = new Movie(Guid.NewGuid(), "Old", new DateOnly(2020, 1, 1), Genre.Drama, 9.99m, 5);
        var cmd = new EditMovie(current.Id, "New", new DateOnly(2024, 6, 1), Genre.SciFi, 12.50m, 8);

        var evt = EditMovieHandler.Handle(cmd, current);

        Assert.Equal("New", evt.Title);
        Assert.Equal(new DateOnly(2024, 6, 1), evt.ReleaseDate);
        Assert.Equal(Genre.SciFi, evt.Genre);
        Assert.Equal(12.50m, evt.Price);
        Assert.Equal(8, evt.Rating);
    }

    [BobcatSpec]
    public void DeleteMovieHandler_returns_MovieDeleted()
    {
        var current = new Movie(Guid.NewGuid(), "Any", new DateOnly(2020, 1, 1), Genre.Drama, 9.99m, 5);
        var evt = DeleteMovieHandler.Handle(new DeleteMovie(current.Id), current);

        Assert.IsType<V1.MovieDeleted>(evt);
    }

    [BobcatSpec]
    public void MovieProjection_Create_from_V2_MovieCreated()
    {
        var id = Guid.NewGuid();
        var created = new MovieCreated(id, "Dune", new DateOnly(2021, 10, 22), Genre.SciFi, 14.99m, 9);

        var movie = MovieProjection.Create(created);

        Assert.Equal(id, movie.Id);
        Assert.Equal("Dune", movie.Title);
        Assert.Equal(9, movie.Rating);
    }

    [BobcatSpec]
    public void MovieProjection_Apply_V2_MovieEdited()
    {
        var current = new Movie(Guid.NewGuid(), "Old", new DateOnly(2020, 1, 1), Genre.Drama, 9.99m, 1);
        var edited = new MovieEdited("New", new DateOnly(2024, 6, 1), Genre.SciFi, 12.50m, 7);

        var updated = MovieProjection.Apply(edited, current);

        Assert.Equal(current.Id, updated.Id);
        Assert.Equal("New", updated.Title);
        Assert.Equal(7, updated.Rating);
    }

}
