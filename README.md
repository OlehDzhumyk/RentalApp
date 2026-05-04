# RentalApp — Peer-to-Peer Rental Marketplace 🛠️📍

[![Build & Test](https://github.com/OlehDzhumyk/RentalApp/actions/workflows/build.yml/badge.svg)](https://github.com/OlehDzhumyk/RentalApp/actions/workflows/build.yml)
[![codecov](https://codecov.io/github/OlehDzhumyk/RentalApp/graph/badge.svg?token=SG288LO14W)](https://codecov.io/github/OlehDzhumyk/RentalApp)

**RentalApp** is a peer-to-peer "Library of Things" marketplace built with **.NET 10 MAUI**. This project serves as a comprehensive demonstration of modern software engineering practices, featuring spatial data processing with **PostGIS**, **MVVM** architecture, and professional **CI/CD** integration.

## 🏗️ Architecture & Tech Stack

The application is built following **Clean Architecture** and **SOLID** principles to ensure maintainability and scalability:

*   **Framework**: .NET 10 MAUI (Cross-platform Android/Windows).
*   **Database**: PostgreSQL 16 with **PostGIS** extension for spatial querying.
*   **ORM**: Entity Framework Core 10 with NetTopologySuite integration.
*   **Patterns**: MVVM (Model-View-ViewModel), Repository Pattern, and Service Layer.
*   **Spatial Logic**: Location-based discovery using `ST_DWithin` and distance calculations via **NetTopologySuite**.

---

## 🚀 Getting Started

### Prerequisites
*   **.NET 10 SDK**.
*   **Docker Desktop** (for running the containerized PostgreSQL environment).
*   **MAUI Workloads**: `dotnet workload install maui-android`.

### Local Development Setup

1.  **Infrastructure**: Spin up the database container from the project root:
    ```bash
    docker-compose up -d
    ```

2.  **Configuration**: Create the local settings file:
    ```bash
    cp RentalApp.Database/appsettings.json.template RentalApp.Database/appsettings.json
    ```
    *Ensure the connection string points to `localhost:5432`.*

3.  **Database Initialization**: Apply migrations to set up the schema and spatial extensions:
    ```bash
    dotnet ef database update --project RentalApp.Migrations --startup-project RentalApp.Migrations
    ```

4.  **Execution**: Launch the application for your target platform:
    ```bash
    # For Android
    dotnet build -t:Run -f net10.0-android
    
    # For Windows
    dotnet build -t:Run -f net10.0-windows10.0.19041.0
    ```

---

## 🧪 Quality Assurance (LO3)

Software quality is maintained through a rigorous testing and automation strategy:

*   **Unit Testing**: Comprehensive test suite using **xUnit** and **Moq**.
*   **CI/CD Pipeline**: GitHub Actions automatically executes builds and tests on an Ubuntu runner, utilizing a service container for PostGIS.
*   **Code Coverage**: Integrated with **Codecov** to monitor and maintain high test coverage standards.

---

## 📝 Documentation
The codebase utilizes XML documentation comments for clarity and is compatible with the **Doxygen** documentation generator tool.