using Microsoft.EntityFrameworkCore;
using NHRMS.Domain;

namespace NHRMS.Infrastructure.Persistence;

public static class NhrmsDbSeeder
{
    public static async Task SeedAsync(NhrmsDbContext db, CancellationToken cancellationToken = default)
    {
        var company = await db.Companies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Code == "NHRMS", cancellationToken);

        if (company is null)
        {
            company = new Company
            {
                Name = "NHRMS",
                Code = "NHRMS",
                TimeZoneId = "Asia/Dhaka"
            };
            db.Companies.Add(company);
            await db.SaveChangesAsync(cancellationToken);
        }
        else if (company.IsDeleted)
        {
            company.IsDeleted = false;
            company.IsActive = true;
        }

        await SeedDepartmentAsync(db, company.Id, "General", "GENERAL", cancellationToken);
        await SeedDesignationAsync(db, company.Id, "Employee", "EMPLOYEE", cancellationToken);
        await SeedEmploymentTypeAsync(db, company.Id, "Full-time", "FULL_TIME", cancellationToken);
        await SeedLeaveTypeAsync(db, company.Id, "Annual leave", "ANNUAL", 20, cancellationToken);
        await SeedLeaveTypeAsync(db, company.Id, "Sick leave", "SICK", 10, cancellationToken);
        await SeedRoleAsync(db, "HR Manager", "HR_MANAGER", "Manages employee records and leave approvals.", cancellationToken);
        await SeedRoleAsync(db, "Payroll Officer", "PAYROLL_OFFICER", "Manages payroll cycles.", cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedDepartmentAsync(
        NhrmsDbContext db,
        Guid companyId,
        string name,
        string code,
        CancellationToken cancellationToken)
    {
        var entity = await db.Departments.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Code == code, cancellationToken);
        if (entity is null)
            db.Departments.Add(new Department { CompanyId = companyId, Name = name, Code = code });
        else
            entity.IsDeleted = false;
    }

    private static async Task SeedDesignationAsync(
        NhrmsDbContext db,
        Guid companyId,
        string name,
        string code,
        CancellationToken cancellationToken)
    {
        var entity = await db.Designations.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Code == code, cancellationToken);
        if (entity is null)
            db.Designations.Add(new Designation { CompanyId = companyId, Name = name, Code = code });
        else
            entity.IsDeleted = false;
    }

    private static async Task SeedEmploymentTypeAsync(
        NhrmsDbContext db,
        Guid companyId,
        string name,
        string code,
        CancellationToken cancellationToken)
    {
        var entity = await db.EmploymentTypes.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Code == code, cancellationToken);
        if (entity is null)
            db.EmploymentTypes.Add(new EmploymentType { CompanyId = companyId, Name = name, Code = code });
        else
            entity.IsDeleted = false;
    }

    private static async Task SeedLeaveTypeAsync(
        NhrmsDbContext db,
        Guid companyId,
        string name,
        string code,
        int days,
        CancellationToken cancellationToken)
    {
        var entity = await db.LeaveTypes.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Code == code, cancellationToken);
        if (entity is null)
            db.LeaveTypes.Add(new LeaveType { CompanyId = companyId, Name = name, Code = code, MaxDaysPerYear = days });
        else
            entity.IsDeleted = false;
    }

    private static async Task SeedRoleAsync(
        NhrmsDbContext db,
        string name,
        string code,
        string description,
        CancellationToken cancellationToken)
    {
        var entity = await db.Roles.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
        if (entity is null)
            db.Roles.Add(new Role { Name = name, Code = code, Description = description });
        else
            entity.IsDeleted = false;
    }
}
