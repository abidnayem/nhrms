using NHRMS.Application.Abstractions;
using NHRMS.Application.Features.Attendance;
using NHRMS.Application.Features.Branches;
using NHRMS.Application.Features.Companies;
using NHRMS.Application.Features.Departments;
using NHRMS.Application.Features.Employees;
using NHRMS.Application.Features.Roles;
using NHRMS.Application.Features.Users;
using NHRMS.Domain;
using System.Security.Cryptography;
using System.Text;

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
        var service = CreateEmployeeService(employeeRepository, companyRepository);

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
        var service = CreateEmployeeService(employeeRepository, companyRepository);

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
    public async Task CreateEmployeeAsync_ShouldRejectInvalidEmail()
    {
        var company = new Company { Name = "Acme", Code = "ACME" };
        var companies = new InMemoryCompanyRepository();
        await companies.AddAsync(company);
        var service = CreateEmployeeService(new InMemoryEmployeeRepository(), companies);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new CreateEmployeeRequest
        {
            CompanyId = company.Id,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "not-an-email",
            DateOfJoining = DateTime.UtcNow.Date
        }));

        Assert.Contains("valid email", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateEmployeeAsync_ShouldRejectMissingJoiningDate()
    {
        var company = new Company { Name = "Acme", Code = "ACME" };
        var companies = new InMemoryCompanyRepository();
        await companies.AddAsync(company);
        var service = CreateEmployeeService(new InMemoryEmployeeRepository(), companies);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new CreateEmployeeRequest
        {
            CompanyId = company.Id,
            FirstName = "Jane",
            LastName = "Doe"
        }));

        Assert.Contains("Joining date is required", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateEmployeeAsync_ShouldRejectDepartmentFromDifferentCompany()
    {
        var company = new Company { Name = "Acme", Code = "ACME" };
        var otherCompany = new Company { Name = "Other", Code = "OTHER" };
        var companies = new InMemoryCompanyRepository();
        await companies.AddAsync(company);
        await companies.AddAsync(otherCompany);
        var departments = new InMemoryDepartmentRepository();
        var department = new Department { CompanyId = otherCompany.Id, Name = "Other department", Code = "OTHER_DEPT" };
        await departments.AddAsync(department);
        var service = CreateEmployeeService(new InMemoryEmployeeRepository(), companies, departments);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new CreateEmployeeRequest
        {
            CompanyId = company.Id,
            FirstName = "Jane",
            LastName = "Doe",
            DateOfJoining = DateTime.UtcNow.Date,
            DepartmentId = department.Id
        }));

        Assert.Contains("belong to the selected company", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateEmployeeAsync_ShouldMarkPreviouslyLeftEmployeeInactive()
    {
        var company = new Company { Name = "Acme", Code = "ACME" };
        var companies = new InMemoryCompanyRepository();
        await companies.AddAsync(company);
        var service = CreateEmployeeService(new InMemoryEmployeeRepository(), companies);

        var employee = await service.CreateAsync(new CreateEmployeeRequest
        {
            CompanyId = company.Id,
            FirstName = "Jane",
            LastName = "Doe",
            DateOfJoining = DateTime.UtcNow.Date.AddYears(-1),
            DateOfLeaving = DateTime.UtcNow.Date.AddDays(-1)
        });

        Assert.False(employee.IsActive);
    }

    [Fact]
    public async Task SearchEmployeesAsync_ShouldFilterAndReturnRequestedPage()
    {
        var company = new Company { Name = "Acme", Code = "ACME" };
        var companies = new InMemoryCompanyRepository();
        await companies.AddAsync(company);
        var employees = new InMemoryEmployeeRepository();
        await employees.AddAsync(new Employee { CompanyId = company.Id, EmployeeCode = "EMP-000001", FirstName = "Jane", LastName = "Doe", DateOfJoining = DateTime.UtcNow.Date });
        await employees.AddAsync(new Employee { CompanyId = company.Id, EmployeeCode = "EMP-000002", FirstName = "Janet", LastName = "Roe", DateOfJoining = DateTime.UtcNow.Date });
        var service = CreateEmployeeService(employees, companies);

        var result = await service.SearchAsync(new EmployeeDirectoryQuery { Search = "Doe", Page = 1, PageSize = 1 });

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Jane", result.Items[0].FirstName);
    }

    [Fact]
    public async Task SearchEmployeesAsync_ShouldRejectUnboundedPageSize()
    {
        var service = CreateEmployeeService(new InMemoryEmployeeRepository(), new InMemoryCompanyRepository());

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SearchAsync(new EmployeeDirectoryQuery { PageSize = 101 }));

        Assert.Contains("between 1 and 100", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateOnboardingTaskAsync_ShouldTrimAndPersistRequiredTask()
    {
        var employee = new Employee { FirstName = "Jane", LastName = "Doe", EmployeeCode = "EMP-1", DateOfJoining = DateTime.UtcNow.Date };
        var employees = new InMemoryEmployeeRepository();
        await employees.AddAsync(employee);
        var tasks = new InMemoryOnboardingTaskRepository();
        var service = new OnboardingService(employees, tasks, new TestCurrentActor(), new NoOpUnitOfWork());

        var task = await service.CreateTaskAsync(employee.Id, new CreateOnboardingTaskRequest
        {
            Title = "  Verify identity  ",
            Description = "  Check the original documents. ",
            IsRequired = true
        });

        Assert.Equal("Verify identity", task.Title);
        Assert.Equal("Check the original documents.", task.Description);
        Assert.Equal(OnboardingTaskStatus.Pending, task.Status);
        Assert.True(task.IsRequired);
    }

    [Fact]
    public async Task UpdateOnboardingTaskStatusAsync_ShouldNotWaiveRequiredTask()
    {
        var employee = new Employee { FirstName = "Jane", LastName = "Doe", EmployeeCode = "EMP-1", DateOfJoining = DateTime.UtcNow.Date };
        var employees = new InMemoryEmployeeRepository();
        await employees.AddAsync(employee);
        var tasks = new InMemoryOnboardingTaskRepository();
        var task = new EmployeeOnboardingTask { EmployeeId = employee.Id, Title = "Verify identity", IsRequired = true };
        await tasks.AddAsync(task);
        var service = new OnboardingService(employees, tasks, new TestCurrentActor(), new NoOpUnitOfWork());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateStatusAsync(employee.Id, task.Id, OnboardingTaskStatus.Waived));

        Assert.Contains("cannot be waived", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateOnboardingTaskStatusAsync_ShouldPreventChangingCompletedTaskAgain()
    {
        var employee = new Employee { FirstName = "Jane", LastName = "Doe", EmployeeCode = "EMP-1", DateOfJoining = DateTime.UtcNow.Date };
        var employees = new InMemoryEmployeeRepository();
        await employees.AddAsync(employee);
        var tasks = new InMemoryOnboardingTaskRepository();
        var task = new EmployeeOnboardingTask { EmployeeId = employee.Id, Title = "Create account", IsRequired = false };
        await tasks.AddAsync(task);
        var service = new OnboardingService(employees, tasks, new TestCurrentActor(), new NoOpUnitOfWork());

        var completed = await service.UpdateStatusAsync(employee.Id, task.Id, OnboardingTaskStatus.Completed);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateStatusAsync(employee.Id, task.Id, OnboardingTaskStatus.Waived));

        Assert.Equal(OnboardingTaskStatus.Completed, completed.Status);
        Assert.NotNull(completed.CompletedAtUtc);
        Assert.Contains("Only pending", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProbationAsync_ShouldKeepRecommendationPendingUntilAnotherActorApproves()
    {
        var employee = new Employee { FirstName = "Jane", LastName = "Doe", EmployeeCode = "EMP-1", DateOfJoining = new DateTime(2026, 1, 1) };
        var employees = new InMemoryEmployeeRepository();
        await employees.AddAsync(employee);
        var probationRepository = new InMemoryProbationRepository();
        var unitOfWork = new NoOpUnitOfWork();
        var reviewer = new TestCurrentActor("reviewer");
        var reviewService = new ProbationService(employees, probationRepository, reviewer, unitOfWork);
        var probationCase = await reviewService.StartAsync(employee.Id, new StartProbationRequest
        {
            StartDate = new DateTime(2026, 1, 1),
            PlannedEndDate = new DateTime(2026, 4, 1)
        });

        var pending = await reviewService.RecommendAsync(probationCase.Id, new CreateProbationReviewRequest
        {
            Recommendation = ProbationRecommendation.Extend,
            Reason = "Additional coaching period",
            ProposedEndDate = new DateTime(2026, 5, 1)
        });
        Assert.Equal(ProbationCaseStatus.InProgress, pending.Status);
        Assert.Equal(ProbationReviewStatus.Pending, Assert.Single(pending.Reviews).Status);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            reviewService.DecideAsync(probationCase.Id, pending.Reviews[0].Id, new DecideProbationReviewRequest { Approve = true }));

        var approverService = new ProbationService(employees, probationRepository, new TestCurrentActor("approver"), unitOfWork);
        var approved = await approverService.DecideAsync(probationCase.Id, pending.Reviews[0].Id,
            new DecideProbationReviewRequest { Approve = true, Notes = "Approved by separate reviewer." });

        Assert.Equal(ProbationCaseStatus.Extended, approved.Status);
        Assert.Equal(new DateTime(2026, 5, 1), approved.ExtendedEndDate);
        Assert.Equal(ProbationReviewStatus.Approved, Assert.Single(approved.Reviews).Status);
        Assert.Equal("approver", approved.Reviews[0].DecidedBy);
    }

    [Fact]
    public async Task ProbationAsync_ShouldRejectOverlappingCaseAndInvalidExtensionDate()
    {
        var employee = new Employee { FirstName = "Jane", LastName = "Doe", EmployeeCode = "EMP-1", DateOfJoining = new DateTime(2026, 1, 1) };
        var employees = new InMemoryEmployeeRepository();
        await employees.AddAsync(employee);
        var repository = new InMemoryProbationRepository();
        var service = new ProbationService(employees, repository, new TestCurrentActor(), new NoOpUnitOfWork());
        var probationCase = await service.StartAsync(employee.Id, new StartProbationRequest
        {
            StartDate = new DateTime(2026, 1, 1),
            PlannedEndDate = new DateTime(2026, 4, 1)
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.StartAsync(employee.Id, new StartProbationRequest
        {
            StartDate = new DateTime(2026, 1, 1),
            PlannedEndDate = new DateTime(2026, 4, 1)
        }));
        await Assert.ThrowsAsync<ArgumentException>(() => service.RecommendAsync(probationCase.Id, new CreateProbationReviewRequest
        {
            Recommendation = ProbationRecommendation.Extend,
            Reason = "Extend",
            ProposedEndDate = new DateTime(2026, 4, 1)
        }));
    }

    [Fact]
    public async Task EmploymentChangeAsync_ShouldKeepFutureApprovalHistoricalUntilEffectiveDate()
    {
        var company = new Company { Name = "Acme", Code = "ACME" };
        var oldDepartment = new Department { CompanyId = company.Id, Name = "Sales", Code = "SALES" };
        var newDepartment = new Department { CompanyId = company.Id, Name = "People", Code = "PEOPLE" };
        var employee = new Employee { CompanyId = company.Id, DepartmentId = oldDepartment.Id, EmployeeCode = "EMP-1", FirstName = "Jane", LastName = "Doe", DateOfJoining = DateTime.UtcNow.Date };
        var employees = new InMemoryEmployeeRepository();
        await employees.AddAsync(employee);
        var departments = new InMemoryDepartmentRepository();
        await departments.AddAsync(oldDepartment);
        await departments.AddAsync(newDepartment);
        var changes = new InMemoryEmploymentChangeRepository();
        var requester = new EmploymentChangeService(employees, departments, new InMemoryDesignationRepository(), new InMemoryEmploymentTypeRepository(), changes, new TestCurrentActor("requester"), new NoOpUnitOfWork());
        var effectiveDate = DateTime.UtcNow.Date.AddDays(1);
        var pending = await requester.RequestAsync(employee.Id, new RequestEmploymentChange { DepartmentId = newDepartment.Id, EffectiveDate = effectiveDate, Reason = "Approved transfer" });

        Assert.Equal(oldDepartment.Id, employee.DepartmentId);
        var approver = new EmploymentChangeService(employees, departments, new InMemoryDesignationRepository(), new InMemoryEmploymentTypeRepository(), changes, new TestCurrentActor("approver"), new NoOpUnitOfWork());
        var approved = await approver.DecideAsync(employee.Id, pending.Id, new DecideEmploymentChange { Approve = true });
        Assert.Equal(EmploymentChangeStatus.Approved, approved.Status);
        Assert.Equal(oldDepartment.Id, employee.DepartmentId);
        Assert.Equal(0, await approver.ApplyDueAsync(employee.Id));

        changes.SetEffectiveDate(pending.Id, DateTime.UtcNow.Date);
        Assert.Equal(1, await approver.ApplyDueAsync(employee.Id));
        Assert.Equal(newDepartment.Id, employee.DepartmentId);
        Assert.Equal(EmploymentChangeStatus.Applied, (await changes.GetChangeAsync(employee.Id, pending.Id))!.Status);
    }

    [Fact]
    public async Task EmploymentChangeAsync_ShouldRequireSeparateApproverAndRejectCrossCompanyDepartment()
    {
        var company = new Company { Name = "Acme", Code = "ACME" };
        var otherCompany = new Company { Name = "Other", Code = "OTHER" };
        var employee = new Employee { CompanyId = company.Id, EmployeeCode = "EMP-1", FirstName = "Jane", LastName = "Doe", DateOfJoining = DateTime.UtcNow.Date };
        var employees = new InMemoryEmployeeRepository();
        await employees.AddAsync(employee);
        var departments = new InMemoryDepartmentRepository();
        var invalidDepartment = new Department { CompanyId = otherCompany.Id, Name = "Other", Code = "OTHER" };
        await departments.AddAsync(invalidDepartment);
        var repository = new InMemoryEmploymentChangeRepository();
        var actor = new TestCurrentActor("requester");
        var service = new EmploymentChangeService(employees, departments, new InMemoryDesignationRepository(), new InMemoryEmploymentTypeRepository(), repository, actor, new NoOpUnitOfWork());
        await Assert.ThrowsAsync<ArgumentException>(() => service.RequestAsync(employee.Id, new RequestEmploymentChange { DepartmentId = invalidDepartment.Id, EffectiveDate = DateTime.UtcNow.Date, Reason = "Transfer" }));

        var validDepartment = new Department { CompanyId = company.Id, Name = "People", Code = "PEOPLE" };
        await departments.AddAsync(validDepartment);
        var request = await service.RequestAsync(employee.Id, new RequestEmploymentChange { DepartmentId = validDepartment.Id, EffectiveDate = DateTime.UtcNow.Date, Reason = "Transfer" });
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DecideAsync(employee.Id, request.Id, new DecideEmploymentChange { Approve = true }));
    }

    [Fact]
    public async Task DisciplinaryCaseAsync_ShouldRestrictDecisionToAnotherActorAndPreserveOutcome()
    {
        var employee = new Employee { EmployeeCode = "EMP-2", FirstName = "Ali", LastName = "Khan", DateOfJoining = DateTime.UtcNow.Date.AddYears(-1) };
        var employees = new InMemoryEmployeeRepository();
        await employees.AddAsync(employee);
        var records = new InMemoryDisciplinaryCaseRepository();
        var opener = new DisciplinaryService(employees, records, new TestCurrentActor("hr-officer"), new NoOpUnitOfWork());
        var item = await opener.CreateAsync(employee.Id, new CreateDisciplinaryCaseRequest
        {
            CaseType = DisciplinaryCaseType.Investigation, OccurredOn = DateTime.UtcNow.Date,
            Subject = "Attendance concern", Summary = "Investigation opened for review."
        });
        await Assert.ThrowsAsync<InvalidOperationException>(() => opener.DecideAsync(employee.Id, item.Id,
            new DecideDisciplinaryCaseRequest { Uphold = true, Decision = "Reviewed" }));

        var decider = new DisciplinaryService(employees, records, new TestCurrentActor("hr-manager"), new NoOpUnitOfWork());
        var resolved = await decider.DecideAsync(employee.Id, item.Id,
            new DecideDisciplinaryCaseRequest { Uphold = false, Decision = "Evidence did not support the allegation." });
        Assert.Equal(DisciplinaryCaseStatus.Dismissed, resolved.Status);
        Assert.Equal("hr-manager", resolved.DecidedBy);
        Assert.Equal("Evidence did not support the allegation.", resolved.Decision);
    }

    private static EmployeeService CreateEmployeeService(
        InMemoryEmployeeRepository employees,
        InMemoryCompanyRepository companies,
        InMemoryDepartmentRepository? departments = null)
        => new(
            employees,
            companies,
            departments ?? new InMemoryDepartmentRepository(),
            new InMemoryDesignationRepository(),
            new InMemoryEmploymentTypeRepository(),
            new TestCurrentActor(),
            new NoOpUnitOfWork());

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
    public async Task AuthenticateAsync_ShouldUpgradeLegacyPasswordHash()
    {
        var password = "LegacyPassword123!";
        var repository = new InMemoryUserAccountRepository();
        var account = new UserAccount
        {
            EmployeeId = Guid.NewGuid(),
            UserName = "legacy-user",
            Email = "legacy@example.com",
            PasswordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)))
        };
        await repository.AddAsync(account);
        var service = new UserAccountService(repository, new NoOpUnitOfWork());

        var authenticated = await service.AuthenticateAsync("legacy-user", password);

        Assert.NotNull(authenticated);
        Assert.StartsWith("PBKDF2-SHA256$", account.PasswordHash, StringComparison.Ordinal);
        Assert.Null(await service.AuthenticateAsync("legacy-user", "incorrect-password"));
    }

    [Fact]
    public async Task BootstrapAsync_ShouldCreateFirstHrManagerEmployeeAndAccount()
    {
        var companyRepository = new InMemoryCompanyRepository();
        var company = new Company { Name = "Acme", Code = "ACME" };
        await companyRepository.AddAsync(company);
        var employeeRepository = new InMemoryEmployeeRepository();
        var userRepository = new InMemoryUserAccountRepository();
        var userRoleRepository = new InMemoryUserRoleRepository();
        var roleRepository = new InMemoryRoleRepository();
        var managerRole = new Role { Name = "HR Manager", Code = "HR_MANAGER" };
        await roleRepository.AddAsync(managerRole);
        var service = new AdminBootstrapService(companyRepository, employeeRepository, userRepository, userRoleRepository, roleRepository, new NoOpUnitOfWork());
        var request = new BootstrapAdminRequest
        {
            CompanyCode = "ACME",
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            UserName = "jane",
            Password = "SecurePassword123!",
            DateOfJoining = new DateTime(2026, 1, 1)
        };

        var user = await service.CreateFirstHrManagerAsync(request);

        Assert.Equal("HR_MANAGER", Assert.Single(user.RoleCodes));
        Assert.Equal("EMP-000001", Assert.Single(await employeeRepository.GetAllAsync()).EmployeeCode);
        Assert.StartsWith("PBKDF2-SHA256$", (await userRepository.GetByUsernameAsync("jane"))!.PasswordHash, StringComparison.Ordinal);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateFirstHrManagerAsync(request));
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

        public Task<(IReadOnlyList<Employee> Items, int TotalCount)> SearchAsync(
            string? search, Guid? companyId, Guid? departmentId, bool? isActive,
            int skip, int take, CancellationToken cancellationToken = default)
        {
            var query = _items.Where(x => !x.IsDeleted);
            if (companyId.HasValue) query = query.Where(x => x.CompanyId == companyId.Value);
            if (departmentId.HasValue) query = query.Where(x => x.DepartmentId == departmentId.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x => x.EmployeeCode.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || x.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || x.LastName.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || (x.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
            var ordered = query.OrderBy(x => x.EmployeeCode).ThenBy(x => x.Id).ToList();
            return Task.FromResult<(IReadOnlyList<Employee> Items, int TotalCount)>((ordered.Skip(skip).Take(take).ToList(), ordered.Count));
        }
    }

    private sealed class InMemoryOnboardingTaskRepository : IEmployeeOnboardingTaskRepository
    {
        private readonly List<EmployeeOnboardingTask> _items = new();
        public Task<EmployeeOnboardingTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.Id == id && !x.IsDeleted));
        public Task<IReadOnlyList<EmployeeOnboardingTask>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<EmployeeOnboardingTask>>(_items.Where(x => !x.IsDeleted).ToList());
        public Task AddAsync(EmployeeOnboardingTask entity, CancellationToken cancellationToken = default)
        { _items.Add(entity); return Task.CompletedTask; }
        public Task UpdateAsync(EmployeeOnboardingTask entity, CancellationToken cancellationToken = default)
        { entity.UpdatedAtUtc = DateTime.UtcNow; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        { var task = _items.FirstOrDefault(x => x.Id == id); if (task is not null) task.IsDeleted = true; return Task.CompletedTask; }
        public Task<IReadOnlyList<EmployeeOnboardingTask>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<EmployeeOnboardingTask>>(_items.Where(x => x.EmployeeId == employeeId && !x.IsDeleted).ToList());
    }

    private sealed class InMemoryProbationRepository : IProbationRepository
    {
        private readonly List<ProbationCase> _cases = new();
        private readonly List<ProbationReview> _reviews = new();
        public Task<ProbationCase?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_cases.FirstOrDefault(x => x.Id == id && !x.IsDeleted));
        public Task<IReadOnlyList<ProbationCase>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ProbationCase>>(_cases.Where(x => !x.IsDeleted).ToList());
        public Task AddAsync(ProbationCase entity, CancellationToken cancellationToken = default)
        { _cases.Add(entity); return Task.CompletedTask; }
        public Task UpdateAsync(ProbationCase entity, CancellationToken cancellationToken = default)
        { entity.UpdatedAtUtc = DateTime.UtcNow; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        { var item = _cases.FirstOrDefault(x => x.Id == id); if (item is not null) item.IsDeleted = true; return Task.CompletedTask; }
        public Task<IReadOnlyList<ProbationCase>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ProbationCase>>(_cases.Where(x => x.EmployeeId == employeeId && !x.IsDeleted).OrderByDescending(x => x.StartDate).ToList());
        public Task<ProbationCase?> GetWithReviewsAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_cases.FirstOrDefault(x => x.Id == id && !x.IsDeleted));
        public Task<ProbationReview?> GetReviewAsync(Guid caseId, Guid reviewId, CancellationToken cancellationToken = default)
            => Task.FromResult(_reviews.FirstOrDefault(x => x.Id == reviewId && x.ProbationCaseId == caseId && !x.IsDeleted));
        public Task<bool> HasPendingReviewAsync(Guid caseId, CancellationToken cancellationToken = default)
            => Task.FromResult(_reviews.Any(x => x.ProbationCaseId == caseId && x.Status == ProbationReviewStatus.Pending && !x.IsDeleted));
        public Task AddReviewAsync(ProbationReview review, CancellationToken cancellationToken = default)
        {
            _reviews.Add(review);
            _cases.First(x => x.Id == review.ProbationCaseId).Reviews.Add(review);
            return Task.CompletedTask;
        }
        public Task UpdateReviewAsync(ProbationReview review, CancellationToken cancellationToken = default)
        { review.UpdatedAtUtc = DateTime.UtcNow; return Task.CompletedTask; }
    }

    private sealed class InMemoryEmploymentChangeRepository : IEmploymentChangeRepository
    {
        private readonly List<EmploymentChange> _items = new();
        public Task<EmploymentChange?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.Id == id && !x.IsDeleted));
        public Task<IReadOnlyList<EmploymentChange>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<EmploymentChange>>(_items.Where(x => !x.IsDeleted).ToList());
        public Task AddAsync(EmploymentChange entity, CancellationToken cancellationToken = default)
        { _items.Add(entity); return Task.CompletedTask; }
        public Task UpdateAsync(EmploymentChange entity, CancellationToken cancellationToken = default)
        { entity.UpdatedAtUtc = DateTime.UtcNow; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        { var item = _items.FirstOrDefault(x => x.Id == id); if (item is not null) item.IsDeleted = true; return Task.CompletedTask; }
        public Task<IReadOnlyList<EmploymentChange>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<EmploymentChange>>(_items.Where(x => x.EmployeeId == employeeId && !x.IsDeleted).OrderByDescending(x => x.EffectiveDate).ToList());
        public Task<EmploymentChange?> GetChangeAsync(Guid employeeId, Guid changeId, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.EmployeeId == employeeId && x.Id == changeId && !x.IsDeleted));
        public Task<bool> HasOpenChangeAsync(Guid employeeId, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.Any(x => x.EmployeeId == employeeId && (x.Status == EmploymentChangeStatus.Pending || x.Status == EmploymentChangeStatus.Approved) && !x.IsDeleted));
        public Task<IReadOnlyList<EmploymentChange>> GetDueApprovedAsync(Guid employeeId, DateTime effectiveOnOrBefore, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<EmploymentChange>>(_items.Where(x => x.EmployeeId == employeeId && x.Status == EmploymentChangeStatus.Approved && x.EffectiveDate <= effectiveOnOrBefore && !x.IsDeleted).ToList());
        public void SetEffectiveDate(Guid id, DateTime effectiveDate) => _items.First(x => x.Id == id).EffectiveDate = effectiveDate;
    }

    private sealed class InMemoryDisciplinaryCaseRepository : IDisciplinaryCaseRepository
    {
        private readonly List<DisciplinaryCase> _items = new();
        public Task<DisciplinaryCase?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.Id == id && !x.IsDeleted));
        public Task<IReadOnlyList<DisciplinaryCase>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<DisciplinaryCase>>(_items.Where(x => !x.IsDeleted).ToList());
        public Task AddAsync(DisciplinaryCase entity, CancellationToken cancellationToken = default)
        { _items.Add(entity); return Task.CompletedTask; }
        public Task UpdateAsync(DisciplinaryCase entity, CancellationToken cancellationToken = default)
        { entity.UpdatedAtUtc = DateTime.UtcNow; return Task.CompletedTask; }
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        { var item = _items.FirstOrDefault(x => x.Id == id); if (item is not null) item.IsDeleted = true; return Task.CompletedTask; }
        public Task<IReadOnlyList<DisciplinaryCase>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<DisciplinaryCase>>(_items.Where(x => x.EmployeeId == employeeId && !x.IsDeleted).ToList());
        public Task<DisciplinaryCase?> GetCaseAsync(Guid employeeId, Guid caseId, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.FirstOrDefault(x => x.EmployeeId == employeeId && x.Id == caseId && !x.IsDeleted));
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

    private sealed class InMemoryDesignationRepository : IDesignationRepository
    {
        private readonly List<Designation> _items = new();
        public Task<Designation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_items.FirstOrDefault(x => x.Id == id && !x.IsDeleted));
        public Task<IReadOnlyList<Designation>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Designation>>(_items.Where(x => !x.IsDeleted).ToList());
        public Task AddAsync(Designation entity, CancellationToken cancellationToken = default) { _items.Add(entity); return Task.CompletedTask; }
        public Task UpdateAsync(Designation entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<Designation?> GetByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default) => Task.FromResult(_items.FirstOrDefault(x => x.CompanyId == companyId && x.Code == code && !x.IsDeleted));
    }

    private sealed class InMemoryEmploymentTypeRepository : IEmploymentTypeRepository
    {
        private readonly List<EmploymentType> _items = new();
        public Task<EmploymentType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_items.FirstOrDefault(x => x.Id == id && !x.IsDeleted));
        public Task<IReadOnlyList<EmploymentType>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<EmploymentType>>(_items.Where(x => !x.IsDeleted).ToList());
        public Task AddAsync(EmploymentType entity, CancellationToken cancellationToken = default) { _items.Add(entity); return Task.CompletedTask; }
        public Task UpdateAsync(EmploymentType entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<EmploymentType?> GetByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default) => Task.FromResult(_items.FirstOrDefault(x => x.CompanyId == companyId && x.Code == code && !x.IsDeleted));
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
            entity.UserId = entity.User?.Id ?? entity.UserId;
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

        public Task<IReadOnlyList<string>> GetPermissionCodesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class TestCurrentActor : ICurrentActor
    {
        public TestCurrentActor(string name = "test-user") => Name = name;
        public string Name { get; }
    }
}
