using NHRMS.Application.Abstractions;
using NHRMS.Application.Features.Attendance;
using NHRMS.Application.Features.Branches;
using NHRMS.Application.Features.Companies;
using NHRMS.Application.Features.Departments;
using NHRMS.Application.Features.Employees;
using NHRMS.Application.Features.Roles;
using NHRMS.Application.Features.Users;
using NHRMS.Domain;

namespace NHRMS.Tests;

public class CoreHumanResourcesTests
{
    [Fact]
    public async Task CreateCompanyAsync_ShouldRejectDuplicateCode()
    {
        var company = new Company { Name = "Acme Ltd", Code = "ACME" };
        var repositories = new InMemoryCompanyRepository();
        await repositories.AddAsync(company);

        var service = new CompanyService(repositories, new NoOpUnitOfWork());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateCompanyRequest
            {
                Name = "Another Acme",
                Code = "ACME"
            }, CancellationToken.None));

        Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateEmployeeAsync_ShouldRejectMissingCompany()
    {
        var employeeRepository = new InMemoryEmployeeRepository();
        var companyRepository = new InMemoryCompanyRepository();
        var service = new EmployeeService(employeeRepository, companyRepository, new NoOpUnitOfWork());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateEmployeeRequest
            {
                CompanyId = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Doe",
                DateOfJoining = DateTime.UtcNow.Date,
                Email = "jane@example.com"
            }, CancellationToken.None));

        Assert.Contains("company", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateEmployeeAsync_ShouldGenerateEmployeeCode()
    {
        var company = new Company { Name = "Acme", Code = "ACME" };
        var employeeRepository = new InMemoryEmployeeRepository();
        var companyRepository = new InMemoryCompanyRepository();
        await companyRepository.AddAsync(company);
        var service = new EmployeeService(employeeRepository, companyRepository, new NoOpUnitOfWork());

        var employee = await service.CreateAsync(new CreateEmployeeRequest
        {
            CompanyId = company.Id,
            FirstName = "Jane",
            LastName = "Doe",
            DateOfJoining = DateTime.UtcNow.Date
        });

        Assert.Equal("EMP-000001", employee.EmployeeCode);
    }

    [Fact]
    public async Task CreateBranchAsync_ShouldRejectDuplicateCode()
    {
        var company = new Company { Id = Guid.NewGuid(), Name = "Acme", Code = "ACME" };
        var companyRepository = new InMemoryCompanyRepository();
        await companyRepository.AddAsync(company);

        var branchRepository = new InMemoryBranchRepository();
        await branchRepository.AddAsync(new Branch
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            Name = "Head Office",
            Code = "HO"
        });

        var service = new BranchService(branchRepository, companyRepository, new NoOpUnitOfWork());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateBranchRequest
            {
                CompanyId = company.Id,
                Name = "Regional Office",
                Code = "HO"
            }, CancellationToken.None));

        Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateDepartmentAsync_ShouldRejectDuplicateCodeWithinCompany()
    {
        var companyId = Guid.NewGuid();
        var companyRepository = new InMemoryCompanyRepository();
        await companyRepository.AddAsync(new Company { Id = companyId, Name = "Acme", Code = "ACME" });

        var departmentRepository = new InMemoryDepartmentRepository();
        await departmentRepository.AddAsync(new Department
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = "Engineering",
            Code = "ENG"
        });

        var service = new DepartmentService(departmentRepository, companyRepository, new InMemoryBranchRepository(), new NoOpUnitOfWork());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateDepartmentRequest
            {
                CompanyId = companyId,
                Name = "Platform",
                Code = "ENG"
            }, CancellationToken.None));

        Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateRoleAsync_ShouldRejectDuplicateCode()
    {
        var repository = new InMemoryRoleRepository();
        await repository.AddAsync(new Role { Id = Guid.NewGuid(), Name = "Admin", Code = "ADMIN" });

        var service = new RoleService(repository, new NoOpUnitOfWork());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateRoleRequest
            {
                Name = "Super Admin",
                Code = "ADMIN"
            }, CancellationToken.None));

        Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAttendanceAsync_ShouldRejectCheckOutBeforeCheckIn()
    {
        var repository = new InMemoryAttendanceRepository();
        var service = new AttendanceService(repository, new NoOpUnitOfWork());

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(new CreateAttendanceRequest
            {
                EmployeeId = Guid.NewGuid(),
                AttendanceDate = DateTime.UtcNow.Date,
                CheckInUtc = DateTime.UtcNow.AddHours(9),
                CheckOutUtc = DateTime.UtcNow.AddHours(8)
            }, CancellationToken.None));

        Assert.Contains("check out", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateUserAccountAsync_ShouldRejectDuplicateUsername()
    {
        var userRepository = new InMemoryUserAccountRepository();
        await userRepository.AddAsync(new UserAccount
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            Email = "admin@acme.com",
            EmployeeId = Guid.NewGuid(),
            IsActive = true
        });

        var service = new UserAccountService(userRepository, new NoOpUnitOfWork());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateUserAccountRequest
            {
                UserName = "admin",
                Email = "new@acme.com",
                EmployeeId = Guid.NewGuid(),
                Password = "Secret@123"
            }, CancellationToken.None));

        Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AssignRoleAsync_ShouldRejectDuplicateAssignment()
    {
        var roleRepository = new InMemoryRoleRepository();
        var userRepository = new InMemoryUserAccountRepository();
        var assignmentRepository = new InMemoryUserRoleRepository();

        var role = new Role { Id = Guid.NewGuid(), Name = "HR Manager", Code = "HRM" };
        var user = new UserAccount { Id = Guid.NewGuid(), UserName = "hr.manager", Email = "hr@acme.com", EmployeeId = Guid.NewGuid(), IsActive = true };

        await roleRepository.AddAsync(role);
        await userRepository.AddAsync(user);
        await assignmentRepository.AddAsync(new UserRole { Id = Guid.NewGuid(), UserId = user.Id, RoleId = role.Id });

        var service = new UserAccountService(userRepository, assignmentRepository, roleRepository, new NoOpUnitOfWork());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AssignRoleAsync(new AssignRoleRequest
            {
                UserId = user.Id,
                RoleId = role.Id
            }, CancellationToken.None));

        Assert.Contains("already assigned", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class InMemoryCompanyRepository : ICompanyRepository
    {
        private readonly List<Company> _items = new();

        public Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.Id == id && !x.IsDeleted));

        public Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Company>>(_items.Where(x => !x.IsDeleted).ToList());

        public Task AddAsync(Company entity, CancellationToken cancellationToken = default)
        {
            _items.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Company entity, CancellationToken cancellationToken = default)
        {
            var index = _items.FindIndex(x => x.Id == entity.Id);
            if (index >= 0) _items[index] = entity;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item is not null) item.IsDeleted = true;
            return Task.CompletedTask;
        }

        public Task<Company?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.Code == code && !x.IsDeleted));
    }

    private sealed class InMemoryEmployeeRepository : IEmployeeRepository
    {
        private readonly List<Employee> _items = new();

        public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.Id == id && !x.IsDeleted));

        public Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Employee>>(_items.Where(x => !x.IsDeleted).ToList());

        public Task AddAsync(Employee entity, CancellationToken cancellationToken = default)
        {
            _items.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Employee entity, CancellationToken cancellationToken = default)
        {
            var index = _items.FindIndex(x => x.Id == entity.Id);
            if (index >= 0) _items[index] = entity;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item is not null) item.IsDeleted = true;
            return Task.CompletedTask;
        }

        public Task<Employee?> GetByEmployeeCodeAsync(string employeeCode, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.EmployeeCode == employeeCode && !x.IsDeleted));

        public Task<long> GetNextEmployeeCodeNumberAsync(CancellationToken cancellationToken = default)
            => Task.FromResult((long)_items.Count + 1);
    }

    private sealed class InMemoryBranchRepository : IBranchRepository
    {
        private readonly List<Branch> _items = new();

        public Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.Id == id && !x.IsDeleted));

        public Task<IReadOnlyList<Branch>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Branch>>(_items.Where(x => !x.IsDeleted).ToList());

        public Task AddAsync(Branch entity, CancellationToken cancellationToken = default)
        {
            _items.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Branch entity, CancellationToken cancellationToken = default)
        {
            var index = _items.FindIndex(x => x.Id == entity.Id);
            if (index >= 0) _items[index] = entity;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item is not null) item.IsDeleted = true;
            return Task.CompletedTask;
        }

        public Task<Branch?> GetByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.CompanyId == companyId && x.Code == code && !x.IsDeleted));
    }

    private sealed class InMemoryDepartmentRepository : IDepartmentRepository
    {
        private readonly List<Department> _items = new();

        public Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.Id == id && !x.IsDeleted));

        public Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Department>>(_items.Where(x => !x.IsDeleted).ToList());

        public Task AddAsync(Department entity, CancellationToken cancellationToken = default)
        {
            _items.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Department entity, CancellationToken cancellationToken = default)
        {
            var index = _items.FindIndex(x => x.Id == entity.Id);
            if (index >= 0) _items[index] = entity;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item is not null) item.IsDeleted = true;
            return Task.CompletedTask;
        }

        public Task<Department?> GetByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.CompanyId == companyId && x.Code == code && !x.IsDeleted));
    }

    private sealed class InMemoryRoleRepository : IRoleRepository
    {
        private readonly List<Role> _items = new();

        public Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.Id == id && !x.IsDeleted));

        public Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Role>>(_items.Where(x => !x.IsDeleted).ToList());

        public Task AddAsync(Role entity, CancellationToken cancellationToken = default)
        {
            _items.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Role entity, CancellationToken cancellationToken = default)
        {
            var index = _items.FindIndex(x => x.Id == entity.Id);
            if (index >= 0) _items[index] = entity;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item is not null) item.IsDeleted = true;
            return Task.CompletedTask;
        }

        public Task<Role?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.Code == code && !x.IsDeleted));
    }

    private sealed class InMemoryAttendanceRepository : IAttendanceRepository
    {
        private readonly List<Attendance> _items = new();

        public Task<Attendance?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.Id == id && !x.IsDeleted));

        public Task<IReadOnlyList<Attendance>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Attendance>>(_items.Where(x => !x.IsDeleted).ToList());

        public Task AddAsync(Attendance entity, CancellationToken cancellationToken = default)
        {
            _items.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Attendance entity, CancellationToken cancellationToken = default)
        {
            var index = _items.FindIndex(x => x.Id == entity.Id);
            if (index >= 0) _items[index] = entity;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item is not null) item.IsDeleted = true;
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryUserAccountRepository : IUserAccountRepository
    {
        private readonly List<UserAccount> _items = new();

        public Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.Id == id && !x.IsDeleted));

        public Task<IReadOnlyList<UserAccount>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UserAccount>>(_items.Where(x => !x.IsDeleted).ToList());

        public Task AddAsync(UserAccount entity, CancellationToken cancellationToken = default)
        {
            _items.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(UserAccount entity, CancellationToken cancellationToken = default)
        {
            var index = _items.FindIndex(x => x.Id == entity.Id);
            if (index >= 0) _items[index] = entity;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item is not null) item.IsDeleted = true;
            return Task.CompletedTask;
        }

        public Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.UserName == username && !x.IsDeleted));

        public Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.Email == email && !x.IsDeleted));

        public Task<UserAccount?> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.EmployeeId == employeeId && !x.IsDeleted));
    }

    private sealed class InMemoryUserRoleRepository : IUserRoleRepository
    {
        private readonly List<UserRole> _items = new();

        public Task<UserRole?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.Id == id && !x.IsDeleted));

        public Task<IReadOnlyList<UserRole>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UserRole>>(_items.Where(x => !x.IsDeleted).ToList());

        public Task AddAsync(UserRole entity, CancellationToken cancellationToken = default)
        {
            _items.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(UserRole entity, CancellationToken cancellationToken = default)
        {
            var index = _items.FindIndex(x => x.Id == entity.Id);
            if (index >= 0) _items[index] = entity;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item is not null) item.IsDeleted = true;
            return Task.CompletedTask;
        }

        public Task<UserRole?> GetByUserAndRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.UserId == userId && x.RoleId == roleId && !x.IsDeleted));

        public Task<IReadOnlyList<UserRole>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UserRole>>(_items.Where(x => x.UserId == userId && !x.IsDeleted).ToList());
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
