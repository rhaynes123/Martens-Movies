# Martens-Movies

A Blazor Web App that replicates the [Microsoft Movie Database tutorial][ms-tutorial],
but swaps EF Core for the [JasperFx][jasperfx] stack:

- **[Marten][marten]** — Postgres event store + inline read-model projection
- **[Wolverine][wolverine]** — in-process command bus using the aggregate handler workflow
- **[Bobcat][bobcat]** — xUnit v3 integration tests projected as Bobcat specifications

Movies are an event-sourced aggregate (`MovieCreated` / `MovieEdited` / `MovieDeleted`),
with versioned events (`V1` → `V2`) converted via a Marten upcaster when the `Rating`
field is added.

## Prerequisites

- .NET 10 SDK
- Docker (for Postgres via `docker compose`)

## Running

```bash
docker compose up -d
dotnet run --project MartensMovies.Web
```

Then open <http://localhost:5000/movies>. Marten's
`ApplyAllDatabaseChangesOnStartup()` creates the schema on first run.

## Tests

```bash
dotnet test
```

Eight specs: four pure-function tests over the aggregate projection and command
handlers, four integration tests that spin up a real Marten+Wolverine host against
the `movies_test` schema (same Postgres instance) and exercise the full event +
projection pipeline, including a `V1 → V2` upcasting round-trip via live
aggregation.

## Project layout

```
MartensMovies.Web/
  Program.cs                      Marten + Wolverine wiring, upcaster registration
  Movies/
    Genre.cs                      Enum: Action/Comedy/Drama/...
    Movie.cs                      Aggregate record (projection target)
    MovieCommands.cs              Create/Edit/Delete + DataAnnotations
    MovieHandlers.cs              [AggregateHandler] workflow handlers
    MovieProjection.cs            SingleStreamProjection<Movie, Guid>, inline, V2 only
    Events/
      V1Events.cs                 Original event shapes
      V2Events.cs                 Current event shapes (add Rating)
      Upcasters.cs                V1 → V2 upcasters (EventUpcaster<TOld, TNew>)
  Components/Pages/Movies/        Index (search + genre filter), Create, Edit, Details, Delete

MartensMovies.Tests/
  MoviesHostFixture.cs            IAsyncLifetime test host (schema=movies_test)
  MovieHandlerTests.cs            Pure-function [BobcatSpec]s
  MovieIntegrationTests.cs        End-to-end [BobcatSpec]s via IMessageBus
```

## Design notes

### Event sourcing with coarse events

A Movie's lifecycle is captured as three event types (`MovieCreated`, `MovieEdited`,
`MovieDeleted`). An inline [`SingleStreamProjection<Movie, Guid>`][marten-projections]
builds the queryable `Movie` document from the stream in the same transaction as the
append, so the Blazor pages can read via `IQuerySession.Query<Movie>()` with no
eventual-consistency surprises.

### Writes via the Wolverine aggregate handler workflow

`EditMovie` and `DeleteMovie` are handled by static methods decorated with
`[AggregateHandler]` — Wolverine+Marten loads the current aggregate state, invokes
the pure function, appends the returned event, and runs the projection. `CreateMovie`
uses a plain handler because there is no existing stream to load. See the
[Wolverine aggregate handler docs][wolverine-aggregate].

### Event versioning

Events live under versioned namespaces (`Movies.Events.V1.*`, `Movies.Events.V2.*`).
Adding the `Rating` field introduced V2 shapes; `MovieCreatedUpcaster` and
`MovieEditedUpcaster` (both deriving from Marten's `EventUpcaster<TOld, TNew>`)
default new fields when the store reads an old event. The upcaster test proves this
round-trips through live aggregation. See
[Marten event schema versioning][marten-upcasters].

### Ponytail flag — inline projection vs upcasting

Upcasters apply when events are **read**, not written. If a raw `V1.MovieCreated` is
appended to a stream with an inline V2-only projection, the inline projection skips
it and the read-model doc is not produced. Live aggregation (`AggregateStreamAsync`)
still works because it reads events through the upcaster. If you ever need to
backfill the inline doc from legacy V1 events, rebuild the projection via the
async daemon or temporarily include V1 handlers.

[ms-tutorial]: https://learn.microsoft.com/en-us/aspnet/core/blazor/tutorials/movie-database-app/
[jasperfx]: https://jasperfx.net
[marten]: https://martendb.io
[wolverine]: https://wolverinefx.net
[bobcat]: https://bobcat.jasperfx.net
[marten-projections]: https://martendb.io/events/projections/
[marten-upcasters]: https://martendb.io/events/versioning.html
[wolverine-aggregate]: https://wolverinefx.net/guide/durability/marten/event-sourcing.html
