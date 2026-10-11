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

## Verification

The solution was validated with `dotnet test NHRMS.slnx --nologo`.

Result: build succeeded and 2 tests passed (0 failed).

> Note: NuGet restore reported existing vulnerability warnings for transitive `System.Security.Cryptography.Xml` 9.0.0. These warnings did not block compilation or execution, but they should be reviewed during the next package hardening pass.
