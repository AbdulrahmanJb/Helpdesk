# Helpdesk

ASP.NET Core helpdesk API project for managing support workflows. The repository is structured as a backend-focused application with API and test projects, making it a good place to show service design, controller organization, and .NET development practices.

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

## Notes

This project is a work in progress. The next polish items are expanding the README with endpoint examples, adding screenshots or API request samples, and documenting the core ticket workflow.
