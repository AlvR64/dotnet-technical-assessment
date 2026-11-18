# Repository guide

- The supplied exercise is `InstruccionesPruebaTecnicaNET.pdf`; the .NET solution and OpenSpec root are under `hexagonal-architecture-doc-main/`. Run the commands below from that directory.
- Preserve the template's original documentation, especially `README.md`. Put project-specific execution and design documentation in `README.PRUEBA.md`.
- Check `openspec/changes/` before implementing: the four REST use cases have separate proposed changes, while the .NET foundation and domain/persistence changes are complete but not archived. Follow each change's specs and tasks; mark only verified tasks complete.

## Build and test

- SDK selection is in `global.json`; package versions are maintained in `Directory.Build.targets`. StyleCop, Sonar and .NET analyzers run during build, with warnings treated as errors.
- `dotnet build src/microservice.sln -c Release`
- Fast tests without PostgreSQL: `dotnet test src/microservice.sln -c Release --filter "Category!=Postgres"`.
- Single test: `dotnet test test/unit/GtMotive.Estimate.Microservice.UnitTests -c Release --filter "FullyQualifiedName~ModelTests"`.
- PostgreSQL tests require `RENTING_TEST_CONNECTION` and a user with `CREATEDB`. Start `docker compose up -d db`, then run `dotnet test src/microservice.sln -c Release --filter "Category=Postgres"`. The functional and migration-startup tests create and delete uniquely named temporary databases; never point them at a server where that is unsafe. See `README.PRUEBA.md` for connection settings.

## Architecture and persistence

- `Host/Program.cs` is the composition root; `Api/ApiConfiguration.cs` registers MediatR, presenters and use cases. Domain must remain independent of EF and HTTP; PostgreSQL mappings and migrations are in `Infrastructure/Persistence/`.
- Vehicle eligibility uses an explicit `DateOnly` reference and `AddYears(-5)`; do not cache availability as a separate flag. A returned rental has `ReturnedAt != null`; partial unique indexes enforce one active rental per customer and per vehicle even under concurrency.
- The API uses `ConnectionStrings__Renting`. Compose opts into `Database__ApplyMigrations=true`; migrations are off by default otherwise. `dotnet tool restore` installs the local EF tool; migration commands and operational caveats are in `README.PRUEBA.md`.
- `/health/live` checks the HTTP process only, not database readiness. The local API has no authentication; Compose credentials are development-only. Never delete existing Docker volumes or databases when testing.
