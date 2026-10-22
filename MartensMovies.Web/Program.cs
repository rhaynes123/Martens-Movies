using JasperFx;
using JasperFx.Events.Projections;
using MartensMovies.Web.Components;
using MartensMovies.Web.Movies;
using MartensMovies.Web.Movies.Events;
using Marten;
using Wolverine;
using Wolverine.Marten;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres");

builder.Services.AddMarten(opts =>
    {
        opts.Connection(connectionString);
        opts.Projections.Add<MovieProjection>(ProjectionLifecycle.Inline);
        opts.Events.Upcast<MovieCreatedUpcaster>();
        opts.Events.Upcast<MovieEditedUpcaster>();
    })
    .IntegrateWithWolverine()
    .UseLightweightSessions()
    .ApplyAllDatabaseChangesOnStartup();

builder.Host.UseWolverine(opts =>
{
    opts.Policies.AutoApplyTransactions();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

return await app.RunJasperFxCommands(args);
