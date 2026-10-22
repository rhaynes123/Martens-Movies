using Bobcat.Xunit;
using MartensMovies.Web.Movies;
using MartensMovies.Web.Movies.Events.V2;
using V1 = MartensMovies.Web.Movies.Events.V1;
using Marten;

namespace MartensMovies.Tests;

public class MovieIntegrationTests : IClassFixture<MoviesHostFixture>, IAsyncLifetime
{
    private readonly MoviesHostFixture _fx;

    public MovieIntegrationTests(MoviesHostFixture fx) => _fx = fx;

    public ValueTask InitializeAsync() => new(_fx.ResetAsync());
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [BobcatSpec]
    public async Task CreateMovie_appends_MovieCreated_and_updates_projection()
    {
        var id = await _fx.Bus.InvokeAsync<Guid>(
            new CreateMovie("Dune", new DateOnly(2021, 10, 22), Genre.SciFi, 14.99m, 9));

        await using var q = _fx.Store.QuerySession();
        var movie = await q.LoadAsync<Movie>(id);
        Assert.NotNull(movie);
        Assert.Equal("Dune", movie!.Title);
        Assert.Equal(Genre.SciFi, movie.Genre);
        Assert.Equal(9, movie.Rating);

        var events = await q.Events.FetchStreamAsync(id);
        Assert.Single(events);
        Assert.IsType<MovieCreated>(events[0].Data);
    }

    [BobcatSpec]
    public async Task EditMovie_appends_MovieEdited_and_updates_projection()
    {
        var id = await _fx.Bus.InvokeAsync<Guid>(
            new CreateMovie("Dune", new DateOnly(2021, 10, 22), Genre.SciFi, 14.99m, 7));

        await _fx.Bus.InvokeAsync(
            new EditMovie(id, "Dune: Part Two", new DateOnly(2024, 3, 1), Genre.SciFi, 16.99m, 10));

        await using var q = _fx.Store.QuerySession();
        var movie = await q.LoadAsync<Movie>(id);
        Assert.Equal("Dune: Part Two", movie!.Title);
        Assert.Equal(16.99m, movie.Price);
        Assert.Equal(10, movie.Rating);

        var events = await q.Events.FetchStreamAsync(id);
        Assert.Equal(2, events.Count);
        Assert.IsType<MovieEdited>(events[1].Data);
    }

    [BobcatSpec]
    public async Task DeleteMovie_removes_projection_and_appends_MovieDeleted()
    {
        var id = await _fx.Bus.InvokeAsync<Guid>(
            new CreateMovie("Dune", new DateOnly(2021, 10, 22), Genre.SciFi, 14.99m, 8));

        await _fx.Bus.InvokeAsync(new DeleteMovie(id));

        await using var q = _fx.Store.QuerySession();
        Assert.Null(await q.LoadAsync<Movie>(id));

        var events = await q.Events.FetchStreamAsync(id);
        Assert.Equal(2, events.Count);
        Assert.IsType<V1.MovieDeleted>(events[1].Data);
    }

    [BobcatSpec]
    public async Task V1_MovieCreated_in_stream_upcasts_to_V2_when_aggregated()
    {
        var id = Guid.NewGuid();

        await using (var writeSession = _fx.Store.LightweightSession())
        {
            writeSession.Events.StartStream<Movie>(id,
                new V1.MovieCreated(id, "Legacy", new DateOnly(2015, 1, 1), Genre.Action, 7.99m));
            await writeSession.SaveChangesAsync();
        }

        await using var q = _fx.Store.QuerySession();

        var events = await q.Events.FetchStreamAsync(id);
        Assert.IsType<MovieCreated>(events[0].Data);
        Assert.Equal(0, ((MovieCreated)events[0].Data).Rating);

        var movie = await q.Events.AggregateStreamAsync<Movie>(id);
        Assert.NotNull(movie);
        Assert.Equal("Legacy", movie!.Title);
        Assert.Equal(0, movie.Rating);
    }
}
