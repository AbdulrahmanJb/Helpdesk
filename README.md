# Helpdesk

ASP.NET Core helpdesk API project for managing support workflows. The repository is backend-focused and demonstrates .NET API structure, controller organization, OpenAPI setup, and a separate test project.

## Portfolio Focus

This project is useful for showing backend fundamentals: API project layout, service-oriented thinking, test structure, and support-system workflow design.

## Tech Stack

| Layer | Technology |
| --- | --- |
| Backend | ASP.NET Core, C#/.NET 10 |
| API Docs | OpenAPI |
| Tests | .NET test project |
| Tooling | .NET CLI, Git, GitHub |

## Repository Structure

```text
Helpdesk.Api/     ASP.NET Core API project
Helpdesk.Tests/   Automated test project
```

## Run Locally

```powershell
dotnet run --project Helpdesk.Api\Helpdesk.Api.csproj
```

When running in development, OpenAPI is mapped by the API application.

## Build

```powershell
dotnet build
```

## Tests

```powershell
dotnet test
```

## Next Improvements

- Document endpoint examples and request/response shapes.
- Add screenshots or OpenAPI examples.
- Expand test coverage around the core ticket workflow.
- Add CI checks for build and tests.
