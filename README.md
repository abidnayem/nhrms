# Smart HRMS Foundation

This repository was initialized to reflect the first implementation phase of the Smart HRMS architecture.

## Architecture summary

- Domain layer: core business entities and base abstractions
- Application layer: repository contracts and unit-of-work abstraction
- Infrastructure layer: EF Core DbContext, repositories, and persistence implementation
- API layer: ASP.NET Core Web API host and service registration
- Test layer: architecture and dependency validation tests

## Default assumptions applied

- Single-company deployment
- Cloud-friendly architecture with SQL Server backing store
- Modular monolith first
- Human Resources, employee lifecycle, attendance, leave, and payroll as the first release scope
- Role-based and permission-based authorization

## Built foundation

- Solution structure created under `NHRMS.slnx`
- Domain model includes Company, Branch, Department, Designation, EmploymentType, and Employee
- Repository and UnitOfWork abstractions are defined and implemented
- EF Core context is configured for the core organization and employee model
- API startup wires up DI, SQL Server, OpenAPI, and health endpoint
- Initial architecture tests validate dependency boundaries

## Running the dynamic HRMS application

The Angular pages read and write employee, attendance, leave, payroll, user, and role records through the API. SQL Server is configured for `DESKTOP-TBBD784`, database `NHRMSDb`, and login `sa`. The password is deliberately stored in .NET User Secrets rather than a tracked settings file:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=DESKTOP-TBBD784;Database=NHRMSDb;User Id=sa;Password=<your-password>;TrustServerCertificate=True;" --project NHRMS.Api
```

On API startup, EF Core applies the checked-in migrations and seeds the initial company and standard department, designation, employment type, leave types, and roles. No sample employee records are inserted.

Run the API and web client in separate terminals:

```powershell
dotnet run --project NHRMS.Api
```

```powershell
cd NHRMS.Web
npm install
npm start
```

The Angular development server uses `http://localhost:4200`; the API listens on `http://localhost:5209`. CORS is configured for the local web origin.

## Verification

The solution was validated with `dotnet test NHRMS.slnx --nologo`.

Result: build succeeded and 2 tests passed (0 failed).

> Note: NuGet restore reported existing vulnerability warnings for transitive `System.Security.Cryptography.Xml` 9.0.0. These warnings did not block compilation or execution, but they should be reviewed during the next package hardening pass.
