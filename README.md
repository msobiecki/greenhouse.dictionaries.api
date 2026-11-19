# greenhouse.dictionaries.api

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](./LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/)

A .NET Web API providing a comprehensive dictionary of plants. It allows applications to discover plant species and access detailed information about their characteristics, growing conditions, and care requirements.

## Table of Contents

- [Features](#features)
- [Getting Started](#getting-started)
- [Tests](#tests)
- [Requirements](#requirements)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [API Documentation](#api-documentation)
- [License](#license)

## Features

- 🌱 🚧 Comprehensive dictionary of plants.
- 🌿 🚧 Detailed plant information and descriptions.
- ☀️ 🚧 Growing conditions and sunlight requirements.
- 💧 🚧 Watering and care recommendations.
- 🌡️ 🚧 Temperature and environmental preferences.
- 🔎 🚧 Plant discovery and search capabilities.

Catalog data is persisted in SQLite during development and SQL Server in the Compose deployment. Read endpoints are anonymous; write endpoints require an API key.

## Getting Started

Follow these instructions to set up the project locally.

### Requirements

- .NET SDK 10.0 or later
- Git
- Docker with Docker Compose (for container deployment)

### Installation

Clone the repository:

```bash
git clone https://github.com/msobiecki/greenhouse.dictionaries.api.git
```

Navigate to the project directory:

```bash
cd greenhouse.dictionaries.api
```

Restore dependencies:

```bash
dotnet restore
```

Build the project:

```bash
dotnet build
```

Run the application:

```bash
export Security__ApiKey="$(openssl rand -hex 32)"
dotnet run --project src/Greenhouse.Dictionaries.Api
```

## Tests

Run the test suite from the repository root:

```bash
dotnet test Greenhouse.Dictionaries.slnx
```

The test project covers command validation, SQLite migrations and search, audit timestamps, API-key authorization, CRUD responses, and exception mapping through the HTTP pipeline.

An opt-in SQL Server integration test verifies production migrations and repository queries. Point it only at a dedicated test database whose name starts with `GreenhouseTests`:

```bash
export GREENHOUSE_SQLSERVER_TEST_CONNECTION='Server=localhost,1433;Database=GreenhouseTestsLocal;User Id=sa;Password=<test-password>;Encrypt=True;TrustServerCertificate=True'
dotnet test tests/Greenhouse.Dictionaries.Api.Tests/Greenhouse.Dictionaries.Api.Tests.csproj --filter FullyQualifiedName~SqlServerPlantRepositoryTests
```

## Configuration

Application settings can be configured using `appsettings.json` and environment variables.

Production uses SQL Server via `ConnectionStrings:Greenhouse`. Supply it using the
`ConnectionStrings__Greenhouse` environment variable; keep credentials out of committed
configuration files. Docker Compose supplies it from `MSSQL_SA_PASSWORD` automatically.

The Development launch profiles use a persistent SQLite database at
`greenhouse-development.db` in the API content directory, so `dotnet run` works without
SQL Server credentials. This local database file is gitignored. To use SQL Server during
development instead, provide `ConnectionStrings__Greenhouse` and set `Database__Provider`
to `SqlServer`.

Write endpoints require the `X-API-Key` header. Set `Security__ApiKey` to a random key
with at least 32 characters for local development and production. Keep it in an
environment variable or secret manager; do not commit it.

```bash
export ConnectionStrings__Greenhouse='Server=localhost,1433;Database=Greenhouse;User Id=sa;Password=<your-password>;Encrypt=True;TrustServerCertificate=True'
export Database__Provider=SqlServer
dotnet run --project src/Greenhouse.Dictionaries.Api --launch-profile http
```

The default development profile listens on HTTP at `http://localhost:5224`.
The optional `https` launch profile also requires a trusted .NET development certificate:

```bash
dotnet dev-certs https --trust
dotnet run --project src/Greenhouse.Dictionaries.Api --launch-profile https
```

### Docker

The Docker Compose configuration runs the application in production mode. Supply an
absolute path to a password-protected PFX certificate and its password through the
environment. The certificate must be readable by the container's non-root user and
valid for the host name used by clients. Use a secret manager to provide the password
in production; do not commit certificates or passwords to the repository.

```bash
export HTTPS_CERTIFICATE_PATH=/absolute/path/to/certificate.pfx
export HTTPS_CERTIFICATE_PASSWORD='your-certificate-password'
export MSSQL_SA_PASSWORD='your-strong-sql-server-password'
export API_KEY="$(openssl rand -hex 32)"
docker compose up --build -d
```

Compose starts a SQL Server container with persistent storage and waits for it to
become healthy before starting the API. On startup, the API creates the `Greenhouse`
database and its initial schema if they do not exist. Keep the SQL Server password
in a secret manager for production; do not commit it to the repository. SQL Server is
bound to localhost for optional host access; the API reaches it through the private
Compose network. `API_KEY` protects catalog write operations.

The API listens on `https://localhost:8443`, and requests to
`http://localhost:8080` are redirected to HTTPS. For a real deployment, replace
`localhost` with the certificate's host name and configure ports and firewall rules
to match the deployment. Stop the container with `docker compose down`.

## Usage

When running in development, the API is available at `http://localhost:5224` and Swagger UI at `http://localhost:5224/swagger`. Swagger is the source of truth for the current routes, parameters, and response schemas as the API evolves.

`Greenhouse.Dictionaries.Api` is the application entry point and composition root. It references the Domain, Application, Infrastructure, and Presentation projects under `src/`.

## License

This project is licensed under the MIT License. See the [LICENSE](./LICENSE) file for more details.

---

Feel free to contribute to this repository by opening issues or submitting pull requests. Happy growing! 🌱

