using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NHRMS.Application.Abstractions;
using NHRMS.Domain;

namespace NHRMS.Infrastructure.Persistence;

public class NhrmsDbContext : DbContext
{
    public NhrmsDbContext(DbContextOptions<NhrmsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<EmploymentType> EmploymentTypes => Set<EmploymentType>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<PayrollCycle> PayrollCycles => Set<PayrollCycle>();
    public DbSet<Payslip> Payslips => Set<Payslip>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeOnboardingTask> EmployeeOnboardingTasks => Set<EmployeeOnboardingTask>();
    public DbSet<ProbationCase> ProbationCases => Set<ProbationCase>();
    public DbSet<ProbationReview> ProbationReviews => Set<ProbationReview>();
    public DbSet<EmploymentChange> EmploymentChanges => Set<EmploymentChange>();
    public DbSet<DisciplinaryCase> DisciplinaryCases => Set<DisciplinaryCase>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasSequence<long>("EmployeeCodeSequence")
            .StartsAt(1)
            .IncrementsBy(1);

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Name);

            // All entities use soft deletion, so database cascades are unnecessary and can
            // produce unsupported multiple cascade paths in SQL Server.
            entity.HasMany(x => x.Branches)
                .WithOne(x => x.Company)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(x => x.Departments)
                .WithOne(x => x.Company)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(x => x.Designations)
                .WithOne(x => x.Company)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(x => x.Employees)
                .WithOne(x => x.Company)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
            entity.HasMany(x => x.Departments).WithOne(x => x.Branch).HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
            entity.HasMany(x => x.ChildDepartments).WithOne(x => x.ParentDepartment).HasForeignKey(x => x.ParentDepartmentId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Designation>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
        });

        modelBuilder.Entity<EmploymentType>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EmployeeCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(200);
            entity.HasIndex(x => new { x.CompanyId, x.EmployeeCode }).IsUnique();

            entity.HasOne(x => x.Department)
                .WithMany(x => x.Employees)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.Designation)
                .WithMany(x => x.Employees)
                .HasForeignKey(x => x.DesignationId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.EmploymentType)
                .WithMany(x => x.Employees)
                .HasForeignKey(x => x.EmploymentTypeId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<EmployeeOnboardingTask>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasIndex(x => new { x.EmployeeId, x.Status, x.DueDate });
            entity.HasOne(x => x.Employee)
                .WithMany(x => x.OnboardingTasks)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ProbationCase>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasIndex(x => x.EmployeeId).IsUnique().HasFilter("[Status] <> 'Confirmed'");
            entity.ToTable(table => table.HasCheckConstraint("CK_ProbationCases_DateRange", "[PlannedEndDate] > [StartDate]"));
            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasMany(x => x.Reviews)
                .WithOne(x => x.ProbationCase)
                .HasForeignKey(x => x.ProbationCaseId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ProbationReview>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Recommendation).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.ReviewedBy).HasMaxLength(100).IsRequired();
            entity.Property(x => x.DecidedBy).HasMaxLength(100);
            entity.Property(x => x.DecisionNotes).HasMaxLength(2000);
            entity.HasIndex(x => x.ProbationCaseId).IsUnique().HasFilter("[Status] = 'Pending'");
        });

        modelBuilder.Entity<EmploymentChange>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ChangeType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.RequestedBy).HasMaxLength(100).IsRequired();
            entity.Property(x => x.DecidedBy).HasMaxLength(100);
            entity.Property(x => x.DecisionNotes).HasMaxLength(2000);
            entity.HasIndex(x => x.EmployeeId).IsUnique().HasFilter("[Status] IN ('Pending', 'Approved')");
            entity.HasIndex(x => new { x.Status, x.EffectiveDate });
            entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<DisciplinaryCase>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CaseType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.Subject).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Summary).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.OpenedBy).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Decision).HasMaxLength(4000);
            entity.Property(x => x.DecidedBy).HasMaxLength(100);
            entity.HasIndex(x => new { x.EmployeeId, x.OccurredOn });
            entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();

            entity.HasMany(x => x.Permissions)
                .WithOne(x => x.Role)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => new { x.RoleId, x.Code }).IsUnique();
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.HoursWorked).HasPrecision(6, 2);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
            entity.HasOne(x => x.Company)
                .WithMany()
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasMany(x => x.LeaveRequests)
                .WithOne(x => x.LeaveType)
                .HasForeignKey(x => x.LeaveTypeId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<PayrollCycle>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TotalGross).HasPrecision(18, 2);
            entity.Property(x => x.TotalDeductions).HasPrecision(18, 2);
            entity.Property(x => x.TotalNet).HasPrecision(18, 2);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.HasOne(x => x.Company)
                .WithMany()
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasMany(x => x.Payslips)
                .WithOne(x => x.PayrollCycle)
                .HasForeignKey(x => x.PayrollCycleId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Payslip>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.GrossSalary).HasPrecision(18, 2);
            entity.Property(x => x.Deductions).HasPrecision(18, 2);
            entity.Property(x => x.NetSalary).HasPrecision(18, 2);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();
            entity.Property(x => x.IsBootstrapAccount).HasDefaultValue(false);
            entity.HasIndex(x => x.UserName).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.EmployeeId).IsUnique();
            entity.HasIndex(x => x.IsBootstrapAccount).IsUnique().HasFilter("[IsBootstrapAccount] = 1");

            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(x => x.UserRoles)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}

public class UnitOfWork : IUnitOfWork
{
    private readonly NhrmsDbContext _dbContext;

    public UnitOfWork(NhrmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);

    public ValueTask DisposeAsync() => _dbContext.DisposeAsync();
}

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly NhrmsDbContext _dbContext;

    public Repository(NhrmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Set<T>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Set<T>().Where(x => !x.IsDeleted).AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await _dbContext.Set<T>().AddAsync(entity, cancellationToken);

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAtUtc = DateTime.UtcNow;
        _dbContext.Set<T>().Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Set<T>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (entity is null)
        {
            return;
        }

        entity.IsDeleted = true;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        _dbContext.Set<T>().Update(entity);
    }
}

public class CompanyRepository : Repository<Company>, ICompanyRepository
{
    public CompanyRepository(NhrmsDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<Company?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim();
        return await _dbContext.Companies
            .FirstOrDefaultAsync(x => x.Code == normalizedCode && !x.IsDeleted, cancellationToken);
    }
}

public class BranchRepository : Repository<Branch>, IBranchRepository
{
    public BranchRepository(NhrmsDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<Branch?> GetByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim();
        return await _dbContext.Branches
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Code == normalizedCode && !x.IsDeleted, cancellationToken);
    }
}

public class DepartmentRepository : Repository<Department>, IDepartmentRepository
{
    public DepartmentRepository(NhrmsDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<Department?> GetByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim();
        return await _dbContext.Departments
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Code == normalizedCode && !x.IsDeleted, cancellationToken);
    }
}

public class DesignationRepository : Repository<Designation>, IDesignationRepository
{
    public DesignationRepository(NhrmsDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<Designation?> GetByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim();
        return await _dbContext.Designations
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Code == normalizedCode && !x.IsDeleted, cancellationToken);
    }
}

public class EmploymentTypeRepository : Repository<EmploymentType>, IEmploymentTypeRepository
{
    public EmploymentTypeRepository(NhrmsDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<EmploymentType?> GetByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim();
        return await _dbContext.EmploymentTypes
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Code == normalizedCode && !x.IsDeleted, cancellationToken);
    }
}

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(NhrmsDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<Role?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim();
        return await _dbContext.Roles
            .FirstOrDefaultAsync(x => x.Code == normalizedCode && !x.IsDeleted, cancellationToken);
    }
}

public class AttendanceRepository : Repository<Attendance>, IAttendanceRepository
{
    public AttendanceRepository(NhrmsDbContext dbContext)
        : base(dbContext)
    {
    }
}

public class LeaveTypeRepository : Repository<LeaveType>, ILeaveTypeRepository
{
    public LeaveTypeRepository(NhrmsDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<LeaveType?> GetByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim();
        return await _dbContext.LeaveTypes
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Code == normalizedCode && !x.IsDeleted, cancellationToken);
    }
}

public class LeaveRequestRepository : Repository<LeaveRequest>, ILeaveRequestRepository
{
    public LeaveRequestRepository(NhrmsDbContext dbContext)
        : base(dbContext)
    {
    }
}

public class PayrollCycleRepository : Repository<PayrollCycle>, IPayrollCycleRepository
{
    public PayrollCycleRepository(NhrmsDbContext dbContext)
        : base(dbContext)
    {
    }
}

public class PayslipRepository : Repository<Payslip>, IPayslipRepository
{
    public PayslipRepository(NhrmsDbContext dbContext)
        : base(dbContext)
    {
    }
}

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(NhrmsDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<Employee?> GetByEmployeeCodeAsync(string employeeCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = employeeCode.Trim();
        return await _dbContext.Employees
            .FirstOrDefaultAsync(x => x.EmployeeCode == normalizedCode && !x.IsDeleted, cancellationToken);
    }

    public async Task<long> GetNextEmployeeCodeNumberAsync(CancellationToken cancellationToken = default)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var shouldCloseConnection = connection.State != System.Data.ConnectionState.Open;

        if (shouldCloseConnection)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT NEXT VALUE FOR dbo.EmployeeCodeSequence";
            if (_dbContext.Database.CurrentTransaction is { } transaction)
                command.Transaction = transaction.GetDbTransaction();

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt64(result);
        }
        finally
        {
            if (shouldCloseConnection)
                await connection.CloseAsync();
        }
    }

    public async Task<(IReadOnlyList<Employee> Items, int TotalCount)> SearchAsync(
        string? search,
        Guid? companyId,
        Guid? departmentId,
        bool? isActive,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Employees.AsNoTracking().Where(x => !x.IsDeleted);
        if (companyId.HasValue)
            query = query.Where(x => x.CompanyId == companyId.Value);
        if (departmentId.HasValue)
            query = query.Where(x => x.DepartmentId == departmentId.Value);
        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.EmployeeCode.Contains(term) ||
                x.FirstName.Contains(term) ||
                x.LastName.Contains(term) ||
                (x.Email != null && x.Email.Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.EmployeeCode)
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }
}

public class EmployeeOnboardingTaskRepository : Repository<EmployeeOnboardingTask>, IEmployeeOnboardingTaskRepository
{
    public EmployeeOnboardingTaskRepository(NhrmsDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<EmployeeOnboardingTask>> GetByEmployeeIdAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
        => await _dbContext.EmployeeOnboardingTasks
            .Where(x => x.EmployeeId == employeeId && !x.IsDeleted)
            .OrderBy(x => x.Status)
            .ThenBy(x => x.DueDate)
            .ThenBy(x => x.Title)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}

public class ProbationRepository : Repository<ProbationCase>, IProbationRepository
{
    public ProbationRepository(NhrmsDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<ProbationCase>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
        => await _dbContext.ProbationCases
            .Where(x => x.EmployeeId == employeeId && !x.IsDeleted)
            .Include(x => x.Reviews.Where(review => !review.IsDeleted))
            .OrderByDescending(x => x.StartDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<ProbationCase?> GetWithReviewsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.ProbationCases
            .Include(x => x.Reviews.Where(review => !review.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public async Task<ProbationReview?> GetReviewAsync(Guid caseId, Guid reviewId, CancellationToken cancellationToken = default)
        => await _dbContext.ProbationReviews
            .FirstOrDefaultAsync(x => x.Id == reviewId && x.ProbationCaseId == caseId && !x.IsDeleted, cancellationToken);

    public Task<bool> HasPendingReviewAsync(Guid caseId, CancellationToken cancellationToken = default)
        => _dbContext.ProbationReviews.AnyAsync(
            x => x.ProbationCaseId == caseId && x.Status == ProbationReviewStatus.Pending && !x.IsDeleted,
            cancellationToken);

    public async Task AddReviewAsync(ProbationReview review, CancellationToken cancellationToken = default)
        => await _dbContext.ProbationReviews.AddAsync(review, cancellationToken);

    public Task UpdateReviewAsync(ProbationReview review, CancellationToken cancellationToken = default)
    {
        _dbContext.ProbationReviews.Update(review);
        return Task.CompletedTask;
    }
}

public class EmploymentChangeRepository : Repository<EmploymentChange>, IEmploymentChangeRepository
{
    public EmploymentChangeRepository(NhrmsDbContext dbContext) : base(dbContext) { }

    public async Task<IReadOnlyList<EmploymentChange>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
        => await _dbContext.EmploymentChanges.AsNoTracking()
            .Where(x => x.EmployeeId == employeeId && !x.IsDeleted)
            .OrderByDescending(x => x.EffectiveDate).ThenByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task<EmploymentChange?> GetChangeAsync(Guid employeeId, Guid changeId, CancellationToken cancellationToken = default)
        => _dbContext.EmploymentChanges.FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.Id == changeId && !x.IsDeleted, cancellationToken);

    public Task<bool> HasOpenChangeAsync(Guid employeeId, CancellationToken cancellationToken = default)
        => _dbContext.EmploymentChanges.AnyAsync(x => x.EmployeeId == employeeId && !x.IsDeleted &&
            (x.Status == EmploymentChangeStatus.Pending || x.Status == EmploymentChangeStatus.Approved), cancellationToken);

    public async Task<IReadOnlyList<EmploymentChange>> GetDueApprovedAsync(Guid employeeId, DateTime effectiveOnOrBefore, CancellationToken cancellationToken = default)
        => await _dbContext.EmploymentChanges
            .Where(x => x.EmployeeId == employeeId && x.Status == EmploymentChangeStatus.Approved && !x.IsDeleted && x.EffectiveDate <= effectiveOnOrBefore)
            .OrderBy(x => x.EffectiveDate).ThenBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
}

public class DisciplinaryCaseRepository : Repository<DisciplinaryCase>, IDisciplinaryCaseRepository
{
    public DisciplinaryCaseRepository(NhrmsDbContext dbContext) : base(dbContext) { }
    public async Task<IReadOnlyList<DisciplinaryCase>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
        => await _dbContext.DisciplinaryCases.Where(x => x.EmployeeId == employeeId && !x.IsDeleted)
            .OrderByDescending(x => x.OccurredOn).ThenByDescending(x => x.OpenedAtUtc).ToListAsync(cancellationToken);
    public Task<DisciplinaryCase?> GetCaseAsync(Guid employeeId, Guid caseId, CancellationToken cancellationToken = default)
        => _dbContext.DisciplinaryCases.FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.Id == caseId && !x.IsDeleted, cancellationToken);
}

public class UserAccountRepository : Repository<UserAccount>, IUserAccountRepository
{
    public UserAccountRepository(NhrmsDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalized = username.Trim();
        return await _dbContext.UserAccounts
            .FirstOrDefaultAsync(x => x.UserName == normalized && !x.IsDeleted, cancellationToken);
    }

    public async Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim();
        return await _dbContext.UserAccounts
            .FirstOrDefaultAsync(x => x.Email == normalized && !x.IsDeleted, cancellationToken);
    }

    public async Task<UserAccount?> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
        => await _dbContext.UserAccounts
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && !x.IsDeleted, cancellationToken);
}

public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
{
    public UserRoleRepository(NhrmsDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<UserRole?> GetByUserAndRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
        => await _dbContext.UserRoles
            .FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId && !x.IsDeleted, cancellationToken);

    public async Task<IReadOnlyList<UserRole>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _dbContext.UserRoles
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<string>> GetPermissionCodesByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await (from userRole in _dbContext.UserRoles.AsNoTracking()
                  join role in _dbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                  join permission in _dbContext.Permissions.AsNoTracking() on role.Id equals permission.RoleId
                  where userRole.UserId == userId && userRole.IsActive && !userRole.IsDeleted
                      && role.IsActive && !role.IsDeleted && permission.IsActive && !permission.IsDeleted
                  select permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);
}
