# AGENTS.md

Rules for AI agents and contributors working in this repository.

## Project

Senswave Web is a Blazor WebAssembly client (.NET 10, MudBlazor) for the Senswave API.
It is served by nginx in a Docker container.

## Verify before you finish

- Always test the Docker container build. CI (`.github/workflows/build.yml`) builds and starts the image, so a local `dotnet build` alone is not enough:

  ```sh
  cd docker
  cp .env.template .env
  docker compose build --no-cache
  docker compose up -d
  docker compose stop
  ```

- Run the unit tests (bUnit, `tests/Senswave.Web.Tests`) and add tests for components you add or change. CI runs them in the `test` job:

  ```sh
  dotnet test tests/Senswave.Web.Tests/Senswave.Web.Tests.csproj
  ```

  Render components through `ComponentTestBase` (MudBlazor services, loose JS interop, popover provider) and substitute services with NSubstitute.
- Run the E2E tests (Playwright, `tests/e2e`) for UI changes. CI runs them against the Docker container in the `e2e` job. By default they start the app with `dotnet run`; set `E2E_BASE_URL=http://localhost:7162` to test the running Docker container:

  ```sh
  cd tests/e2e
  npm install
  npx playwright install chromium
  npx playwright test
  ```

  Tests use the shared test API (`https://apisenswave.psplace.dev`, override with `E2E_API_URL`). Never create real accounts or send email to real people: mock mutating calls with `page.route` and use `@example.com` addresses. Signed-in tests use the test account from `E2E_USER_EMAIL` / `E2E_USER_PASSWORD` (GitHub secrets in CI) and skip when these are not set.
- The Docker build uses `Release` configuration on Linux. Paths and file names are case-sensitive there.
- Build this checkout, not another copy. Keep project paths in `Senswave.Web.slnx` and `.csproj` files relative.
- When you add a project, also add its `.csproj` `COPY` line to `src/Senswave.Web/dockerfile`, or restore fails in Docker.
- Do not leave `Compile Remove` / `Content Remove` entries for folders that should be built.

## Modular architecture

```
src/
  Senswave.Web/            Host app: pages, layouts, app-level components, service implementations, DI in Program.cs
  Modules/Senswave.Web.*/  Feature modules (Users, Homes, Devices, DataSources, LiveUpdate)
  Shared/Senswave.Web.Shared/  Cross-cutting code: Result/Error, storage, loading, request handlers
```

- Each module owns its feature: `Integration/` (Refit API clients), `Models/`, `Services/` (interfaces), `Components/` (reusable Razor components).
- Each module registers its Refit clients in a `<Module>Extensions.Add<Module>(IServiceCollection, IConfiguration)` method. The host calls it from `Program.cs`.
- Modules do not reference other modules. Put shared code in `Senswave.Web.Shared`.
- Modules must not reference the host project. The host depends on modules, never the reverse.
- New feature code goes into the matching module. Put code in the host only when it wires modules together (pages, layout, service implementations).
- API routes include the version (`/api/v1/...`, `/api/v2/...`). Match the version the backend maps.

## .NET code practices

- Match the style of the surrounding code: file-scoped namespaces, primary constructors, `Async` suffix where already used.
- Nullable reference types are enabled. Do not add new nullable warnings. Do not suppress them with `!` unless the value is guaranteed.
- Return `Result` / `Result<T>` from services instead of throwing. Catch exceptions at the integration boundary, log them, and return an error from `IErrorFactory`.
- Use structured logging with message templates (`logger.LogError(ex, "Failed for {UserId}", id)`), not string interpolation.
- Use `async`/`await` all the way. Do not use `.Result` or `.Wait()` on tasks that have not completed.
- Inject dependencies through constructors or `@inject`. Do not resolve services manually.
- Keep secrets and environment-specific values out of source. Runtime config comes from `docker/.env` through `docker-entrypoint.d/40-appsettings.sh`.
- Unsubscribe from events in `Dispose` when a component subscribes to service events.
- Keep changes small and focused. Do not reformat unrelated code.
