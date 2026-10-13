using NHRMS.Domain;

namespace NHRMS.Application.Abstractions;

public interface ICurrentActor
{
    string Name { get; }
}

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
    Task<long> GetNextEmployeeCodeNumberAsync(CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Employee> Items, int TotalCount)> SearchAsync(
        string? search,
        Guid? companyId,
        Guid? departmentId,
        bool? isActive,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
}

public interface IEmployeeOnboardingTaskRepository : IRepository<EmployeeOnboardingTask>
{
    Task<IReadOnlyList<EmployeeOnboardingTask>> GetByEmployeeIdAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);
}

public interface IProbationRepository : IRepository<ProbationCase>
{
    Task<IReadOnlyList<ProbationCase>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<ProbationCase?> GetWithReviewsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProbationReview?> GetReviewAsync(Guid caseId, Guid reviewId, CancellationToken cancellationToken = default);
    Task<bool> HasPendingReviewAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task AddReviewAsync(ProbationReview review, CancellationToken cancellationToken = default);
    Task UpdateReviewAsync(ProbationReview review, CancellationToken cancellationToken = default);
}

public interface IEmploymentChangeRepository : IRepository<EmploymentChange>
{
    Task<IReadOnlyList<EmploymentChange>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<EmploymentChange?> GetChangeAsync(Guid employeeId, Guid changeId, CancellationToken cancellationToken = default);
    Task<bool> HasOpenChangeAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmploymentChange>> GetDueApprovedAsync(Guid employeeId, DateTime effectiveOnOrBefore, CancellationToken cancellationToken = default);
}

public interface IDisciplinaryCaseRepository : IRepository<DisciplinaryCase>
{
    Task<IReadOnlyList<DisciplinaryCase>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<DisciplinaryCase?> GetCaseAsync(Guid employeeId, Guid caseId, CancellationToken cancellationToken = default);
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
    Task<IReadOnlyList<string>> GetPermissionCodesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
