# MunPath

## Overview
MunPath is a web application built with **ASP.NET Core MVC**, **Entity Framework Core**, and **Supabase PostgreSQL**. It is designed with a structured architecture to support maintainability, data management, and future feature development.

## Technologies
- C#
- ASP.NET Core MVC
- Entity Framework Core
- Supabase PostgreSQL
- Razor Views

## Features
- User and role management models
- Signup-related DTOs, mapping, and service logic
- Password hashing and email-verification status handling
- Database relationships and schema migrations

*Features listed should be verified against the current implementation.*

## Project Structure
- `Controllers/` — Handles application requests.
- `Models/` — Defines application data models.
- `DTOs/` — Structures request and response data.
- `Services/` — Contains application logic.
- `Mappers/` — Converts between DTOs and models.
- `Data/` — Handles database-related configuration.
- `Migrations/` — Tracks database schema changes.
- `Views/` — Contains the application's user interface.
- `Extensions/` — Provides reusable extension methods and helpers.

## Getting Started

### Prerequisites
- Compatible .NET SDK
- Git
- Access to the configured PostgreSQL database

### Run Locally
```bash
dotnet restore
dotnet build
dotnet run
```

Configure the database connection before running the application. Keep credentials and other secrets out of source control.

## Project Status
MunPath is under development. Features, requirements, and implementation details may evolve as development continues.
