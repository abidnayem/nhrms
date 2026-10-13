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
- Employee directory has company/department/status filters, free-text search, and bounded pagination
- Employee onboarding checklist and probation review/approval workflows are connected to the Angular UI
- Employment assignment changes retain history, require a separate approver, and support future effective dates
- Employee-relations cases use dedicated read/manage/decision permissions and separate case openers from decision makers
- JWT authentication, permission policies, first-time HR manager setup, and actor stamping protect lifecycle endpoints
- API startup wires up DI, SQL Server, OpenAPI, and health endpoint
- Initial architecture tests validate dependency boundaries

## Running the dynamic HRMS application

The Angular pages read and write employee, attendance, leave, payroll, user, and role records through the API. SQL Server is configured for `DESKTOP-TBBD784`, database `NHRMSDb`, and login `sa`. The password is deliberately stored in .NET User Secrets rather than a tracked settings file:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=DESKTOP-TBBD784;Database=NHRMSDb;User Id=sa;Password=<your-password>;TrustServerCertificate=True;" --project NHRMS.Api
```

In Development, the API applies the checked-in migrations and seeds the initial company and standard department, designation, employment type, leave types, roles, and role permissions at startup. Production startup does not apply migrations. After reviewing the migration, production operators can explicitly migrate and seed with:

```powershell
dotnet run --project NHRMS.Api -- --migrate-and-seed
```

The API requires a JWT signing key of at least 32 UTF-8 bytes. Development generates an ephemeral key if one is not configured (existing browser tokens then expire after an API restart). To use stable development credentials and enable one-time HR manager setup, configure secrets:

```powershell
dotnet user-secrets set "Authentication:JwtKey" "<random-secret-at-least-32-bytes>" --project NHRMS.Api
dotnet user-secrets set "Authentication:BootstrapKey" "<separate-random-secret-at-least-32-bytes>" --project NHRMS.Api
```

Open the web app and choose **First-time setup** to create the first HR manager employee and account. This endpoint requires the bootstrap key and is disabled when the key is not configured; it also becomes unavailable after an account exists. Use a strong password of at least 12 characters. Sign-in tokens expire after 60 minutes and are kept in the browser tab session. HR API endpoints require bearer authentication and seeded role permissions.

Run the API and web client in separate terminals:

```powershell
dotnet run --project NHRMS.Api
```

For local API diagnostics when you need to keep Development startup from applying migrations, run `dotnet run --project NHRMS.Api -- --skip-startup-migrate`. The API will still need a reachable database with the current schema for database-backed endpoints.

```powershell
cd NHRMS.Web
npm install
npm start
```

The Angular development server uses `http://localhost:4200`; the API listens on `http://localhost:5209`. CORS is configured for the local web origin.

## Verification

The solution was validated with `dotnet build NHRMS.slnx --nologo --no-restore -m:1`, `dotnet test NHRMS.slnx --nologo --no-restore -m:1`, and `npm run build` in `NHRMS.Web`.

Latest result: solution build succeeded, 27 tests passed (0 failed), and Angular production build succeeded. EF reports no model changes pending. Additive onboarding, bootstrap-account, probation, employment-change, and restricted discipline migrations are checked in but have not been applied to the configured database. A SQL Server encryption handshake failure prevented database connectivity and database-backed API smoke testing in this environment.

See [docs/personnel-lifecycle-audit.md](docs/personnel-lifecycle-audit.md) for the feature inventory, remaining lifecycle gaps, and business decisions that need owners. The implemented slices do not yet constitute the full personnel lifecycle: recruitment handover, employee document storage, compensation/contract/grade/branch/manager changes, disciplinary evidence/retention, separation/clearance/final settlement, and lifecycle reports remain outstanding.

> Note: NuGet restore reported existing vulnerability warnings for transitive `System.Security.Cryptography.Xml` 9.0.0. These warnings did not block compilation or execution, but they should be reviewed during the next package hardening pass.
