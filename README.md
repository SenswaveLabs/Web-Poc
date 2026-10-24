# Proof Of Concept - Sensawve Web

The application was created to explore the capabilities of Senswave in a browser environment, identify potential issues, and redesign the user interface. It also aims to apply WCAG accessibility standards in a practical and straightforward way, achieving a strong score (approximately 9/10) using the WAVE evaluation tool.

Originally, Senswave was intended exclusively for mobile devices. However, considering the possibility of running the solution locally also opens the door to providing a simple web-based interface, making it easier to use and manage the system.

This project should be considered a simple Proof of Concept (PoC) rather than a production-ready solution for Senswave.

## Build With

![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![.NET 10](https://img.shields.io/badge/.NET_10-5C2D91?style=for-the-badge&logo=dotnet&logoColor=white)
![Blazor WASM](https://img.shields.io/badge/Blazor_WASM-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Refit](https://img.shields.io/badge/Refit-FF6C37?style=for-the-badge&logo=dotnet&logoColor=white)
![MudBlazor](https://img.shields.io/badge/MudBlazor-1E88E5?style=for-the-badge&logo=dotnet&logoColor=white)

## Project Structure

```text
├── src/               # Blazor WASM Source
│   ├── Modules/       # Domain-specific logic (Devices, Users, Homes), Refit REST API contracts & SignalR Hub clients
│   ├── Shared/        # Shared components between modules
│   └── Senswave.Web/  # Main application code with interfaces implementations
├── docs/              # Technical documentation (Polish)
└── docker/            # Web Application Deployment
```

## How to Run

This application depends on the Senswave API. A shared test instance is available at `https://apisenswave.psplace.dev` and is the default in `docker/.env.template`.

> The test API does not allow `localhost` origins in CORS yet, so a regular browser blocks its calls from `http://localhost:7162`. The E2E tests work around this with `--disable-web-security`. For manual testing, either run the API locally (`API_BASE_ADDRESS=http://localhost:7070`) or ask for `http://localhost:7162` to be added to the API's allowed origins.

### Run Web Application with Docker

1. **Clone the repository and go to the Docker setup:**

```bash
cd docker
cp .env.template .env
```

2. **Set the values in `.env`:**

| Variable | Description |
| --- | --- |
| `BUILD_CONFIGURATION` | `Release` (same as CI) or `Debug` |
| `API_BASE_ADDRESS` | Senswave API, e.g. `https://apisenswave.psplace.dev` or `http://localhost:7070` |
| `GOOGLE_CLIENT_ID` | Google OAuth client id. Leave empty to disable Google sign-in |

The values are written to `appsettings.json` when the container starts, so changing them only needs a restart, not a rebuild.

3. **Launch Environment:**

```bash
docker compose up -d --build
```

The app is available at `http://localhost:7162`.

### E2E Tests

Playwright tests live in `tests/e2e`. They point the app at the test API by themselves (override with `E2E_API_URL`), so they do not depend on `API_BASE_ADDRESS`.

```bash
cd tests/e2e
npm install
npx playwright install chromium
npx playwright test                                     # starts the app with dotnet run
E2E_BASE_URL=http://localhost:7162 npx playwright test  # tests the running Docker container
```

Most tests mock calls that create accounts or send email. The signed-in test (`session.spec.ts`) uses a real account on the test API and is skipped when no account is configured.

#### Test account for local runs

1. Register an account on the test API with a mailbox you own (not `@example.com`, the confirmation link is sent by email) and confirm it. Accept the terms after the first sign-in.
2. Set the credentials in your shell before running the tests. Do not commit them.

```bash
# bash
export E2E_USER_EMAIL="you@yourdomain.com"
export E2E_USER_PASSWORD="your-password"
```

```powershell
# PowerShell
$env:E2E_USER_EMAIL = "you@yourdomain.com"
$env:E2E_USER_PASSWORD = "your-password"
```

#### Test account for GitHub Actions

The `e2e` job in `.github/workflows/build.yml` reads the account from repository secrets. Use a dedicated test account, not a personal one.

1. Open **Settings → Secrets and variables → Actions → New repository secret**.
2. Add `E2E_USER_EMAIL` and `E2E_USER_PASSWORD`.

Or with the GitHub CLI:

```bash
gh secret set E2E_USER_EMAIL --body "e2e@yourdomain.com"
gh secret set E2E_USER_PASSWORD
```

Secrets are not passed to workflows triggered from forks, so the signed-in test is skipped on fork pull requests.

## License

Distributed under the **Apache License 2.0**. See `LICENSE` for more information.