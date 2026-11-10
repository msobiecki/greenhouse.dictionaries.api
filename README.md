# greenhouse.dictionaries.api

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](./LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/)

A .NET Web API providing a comprehensive dictionary of flowers and plants. It allows applications to discover plant species and access detailed information about their characteristics, growing conditions, and care requirements.

## Table of Contents

- [Features](#features)
- [Getting Started](#getting-started)
- [Requirements](#requirements)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [API Documentation](#api-documentation)
- [License](#license)
- [Contributing](#contributing)

## Features

- 🌱 Comprehensive dictionary of flowers and plants.
- 🌿 Detailed plant information and descriptions.
- ☀️ Growing conditions and sunlight requirements.
- 💧 Watering and care recommendations.
- 🌡️ Temperature and environmental preferences.
- 🔎 Plant discovery and search capabilities.
- 🔌 RESTful API for easy integration with other applications.

## Getting Started

Follow these instructions to set up the project locally.

### Requirements

- .NET SDK 10.0 or later
- Git

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
dotnet run
```

## Configuration

Application settings can be configured using `appsettings.json` and environment variables.

For local development, you can use `appsettings.Development.json` to override default settings.

## Usage

Once the application is running, the API will be available at the configured local address.

For example:

```text
https://localhost:7000
http://localhost:5000
```

The API exposes RESTful endpoints for accessing plant and flower information.

Example request:

```http
GET /api/plants
```

Example response:

```json
[
  {
    "id": 1,
    "name": "Monstera deliciosa",
    "commonName": "Swiss Cheese Plant",
    "family": "Araceae",
    "description": "A tropical plant known for its distinctive split leaves."
  }
]
```

*Note: API endpoints, response structures, and available features are subject to change during development.*

## API Documentation

API documentation will be available through Swagger / OpenAPI when running the application in development mode.

## License

This project is licensed under the MIT License. See the [LICENSE](./LICENSE) file for more details.

---

## Contributing

Feel free to contribute to this repository by opening issues or submitting pull requests. Happy growing! 🌱
