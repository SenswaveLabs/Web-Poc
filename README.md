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

This application depends on the Senswave API, which is currently restricted to organization members.

To run the project in standalone mode, you will need to provide your own mock implementations of the services. This can be done easily using the provided REST Refit interfaces and any modern AI tooling. There are plans to make the API and related components publicly available in the future.

### Run Web Application with Docker

1. **Clone the repository:**

2. **Navigate to Docker setup:**

```bash
cd docker
```

3. **Launch Environment:**

```bash
docker-compose up -d
```

*Note: Ensure your `.env` file contains the correct values.*

## License

Distributed under the **Apache License 2.0**. See `LICENSE` for more information.