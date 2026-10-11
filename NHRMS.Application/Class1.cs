using NHRMS.Domain;

namespace NHRMS.Application.Abstractions;

public interface IUnitOfWork : IAsyncDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface ICompanyRepository : IRepository<Company>
{
    Task<Company?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}

public interface IBranchRepository : IRepository<Branch>
{
    Task<Branch?> GetByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
}

public interface IDepartmentRepository : IRepository<Department>
{
    Task<Department?> GetByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
}

public interface IDesignationRepository : IRepository<Designation>
{
    Task<Designation?> GetByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
}

public interface IEmploymentTypeRepository : IRepository<EmploymentType>
{
    Task<EmploymentType?> GetByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
}

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}

public interface IAttendanceRepository : IRepository<Attendance>
{
}

public interface ILeaveTypeRepository : IRepository<LeaveType>
{
    Task<LeaveType?> GetByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
}

public interface ILeaveRequestRepository : IRepository<LeaveRequest>
{
}

public interface IPayrollCycleRepository : IRepository<PayrollCycle>
{
}

public interface IPayslipRepository : IRepository<Payslip>
{
}

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<Employee?> GetByEmployeeCodeAsync(string employeeCode, CancellationToken cancellationToken = default);
}

public interface IUserAccountRepository : IRepository<UserAccount>
{
    Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserAccount?> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);
}

public interface IUserRoleRepository : IRepository<UserRole>
{
    Task<UserRole?> GetByUserAndRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserRole>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
