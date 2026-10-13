using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NHRMS.Application.Abstractions;
using NHRMS.Infrastructure.Persistence;

namespace NHRMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNhrmsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NhrmsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IDesignationRepository, DesignationRepository>();
        services.AddScoped<IEmploymentTypeRepository, EmploymentTypeRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<IPayrollCycleRepository, PayrollCycleRepository>();
        services.AddScoped<IPayslipRepository, PayslipRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IEmployeeOnboardingTaskRepository, EmployeeOnboardingTaskRepository>();
        services.AddScoped<IProbationRepository, ProbationRepository>();
        services.AddScoped<IEmploymentChangeRepository, EmploymentChangeRepository>();
        services.AddScoped<IDisciplinaryCaseRepository, DisciplinaryCaseRepository>();
        services.AddScoped<IUserAccountRepository, UserAccountRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
