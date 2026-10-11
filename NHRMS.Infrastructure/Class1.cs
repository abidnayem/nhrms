using Microsoft.EntityFrameworkCore;
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
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Name);

            entity.HasMany(x => x.Branches)
                .WithOne(x => x.Company)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Departments)
                .WithOne(x => x.Company)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Designations)
                .WithOne(x => x.Company)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Employees)
                .WithOne(x => x.Company)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
            entity.HasMany(x => x.Departments).WithOne(x => x.Branch).HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
            entity.HasMany(x => x.ChildDepartments).WithOne(x => x.ParentDepartment).HasForeignKey(x => x.ParentDepartmentId).OnDelete(DeleteBehavior.Restrict);
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
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.Designation)
                .WithMany(x => x.Employees)
                .HasForeignKey(x => x.DesignationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.EmploymentType)
                .WithMany(x => x.Employees)
                .HasForeignKey(x => x.EmploymentTypeId)
                .OnDelete(DeleteBehavior.SetNull);
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
                .OnDelete(DeleteBehavior.Cascade);
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
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
            entity.HasMany(x => x.LeaveRequests)
                .WithOne(x => x.LeaveType)
                .HasForeignKey(x => x.LeaveTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PayrollCycle>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.HasMany(x => x.Payslips)
                .WithOne(x => x.PayrollCycle)
                .HasForeignKey(x => x.PayrollCycleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payslip>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => x.UserName).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.EmployeeId).IsUnique();

            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.UserRoles)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
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
}
