using JasperFx.Events.Projections;
using MartensMovies.Web.Movies;
using MartensMovies.Web.Movies.Events;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.Marten;
using HostBuilder = Microsoft.Extensions.Hosting.Host;

namespace MartensMovies.Tests;

public sealed class MoviesHostFixture : IAsyncLifetime
{
    public IHost Host { get; private set; } = default!;

    public IDocumentStore Store => Host.Services.GetRequiredService<IDocumentStore>();
    public IMessageBus Bus => Host.Services.GetRequiredService<IMessageBus>();

    public ValueTask InitializeAsync()
    {
        var builder = HostBuilder.CreateDefaultBuilder();

        builder.ConfigureServices(services =>
        {
            services.AddMarten(opts =>
                {
                    opts.Connection(
                        "Host=localhost;Port=5433;Username=movies;Password=movies;Database=movies");
                    opts.DatabaseSchemaName = "movies_test";
                    opts.Projections.Add<MovieProjection>(ProjectionLifecycle.Inline);
                    opts.Events.Upcast<MovieCreatedUpcaster>();
                    opts.Events.Upcast<MovieEditedUpcaster>();
                })
                .IntegrateWithWolverine()
                .UseLightweightSessions()
                .ApplyAllDatabaseChangesOnStartup();
        });

        builder.UseWolverine(opts =>
        {
            opts.Discovery.IncludeAssembly(typeof(CreateMovie).Assembly);
            opts.Policies.AutoApplyTransactions();
        });

        var host = builder.Build();
        Host = host;
        return new ValueTask(host.StartAsync());
    }

    public async ValueTask DisposeAsync()
    {
        await Host.StopAsync();
        Host.Dispose();
    }

    public Task ResetAsync() => Store.Advanced.Clean.DeleteAllDocumentsAsync()
        .ContinueWith(_ => Store.Advanced.Clean.DeleteAllEventDataAsync()).Unwrap();
}
