using System.Net.Mail;
using NHRMS.Application.Abstractions;
using NHRMS.Domain;

namespace NHRMS.Application.Features.Users;

public sealed class AdminBootstrapService(
    ICompanyRepository companyRepository,
    IEmployeeRepository employeeRepository,
    IUserAccountRepository userRepository,
    IUserRoleRepository userRoleRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<AuthenticatedUserDto> CreateFirstHrManagerAsync(
        BootstrapAdminRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if ((await userRepository.GetAllAsync(cancellationToken)).Count != 0)
            throw new InvalidOperationException("Administrator setup has already been completed.");

        var companyCode = request.CompanyCode.Trim();
        var company = await companyRepository.GetByCodeAsync(companyCode, cancellationToken);
        if (company is null || !company.IsActive)
            throw new ArgumentException("An active company with the supplied code is required.", nameof(request.CompanyCode));

        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        var email = request.Email.Trim();
        var userName = request.UserName.Trim();
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("First and last names are required.", nameof(request.FirstName));
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("A user name is required.", nameof(request.UserName));
        if (!IsValidEmail(email))
            throw new ArgumentException("A valid email address is required.", nameof(request.Email));
        if (request.DateOfJoining == default)
            throw new ArgumentException("Joining date is required.", nameof(request.DateOfJoining));
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 12)
            throw new ArgumentException("The administrator password must be at least 12 characters long.", nameof(request.Password));
        if (await userRepository.GetByUsernameAsync(userName, cancellationToken) is not null)
            throw new InvalidOperationException("User name already exists.");
        if (await userRepository.GetByEmailAsync(email, cancellationToken) is not null)
            throw new InvalidOperationException("Email already exists.");

        var role = await roleRepository.GetByCodeAsync("HR_MANAGER", cancellationToken);
        if (role is null || !role.IsActive)
            throw new InvalidOperationException("The active HR_MANAGER role is required before administrator setup.");

        var employee = new Employee
        {
            CompanyId = company.Id,
            EmployeeCode = $"EMP-{await employeeRepository.GetNextEmployeeCodeNumberAsync(cancellationToken):D6}",
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            DateOfJoining = request.DateOfJoining.Date,
            IsActive = true,
            CreatedBy = "bootstrap"
        };
        var account = new UserAccount
        {
            EmployeeId = employee.Id,
            Employee = employee,
            UserName = userName,
            Email = email,
            PasswordHash = PasswordHashing.Hash(request.Password),
            IsActive = true,
            IsBootstrapAccount = true,
            CreatedBy = "bootstrap"
        };
        var assignment = new UserRole
        {
            UserId = account.Id,
            User = account,
            RoleId = role.Id,
            IsActive = true,
            CreatedBy = "bootstrap"
        };
        await employeeRepository.AddAsync(employee, cancellationToken);
        await userRepository.AddAsync(account, cancellationToken);
        await userRoleRepository.AddAsync(assignment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var permissions = await userRoleRepository.GetPermissionCodesByUserIdAsync(account.Id, cancellationToken);
        return new AuthenticatedUserDto(new UserAccountDto
        {
            Id = account.Id,
            EmployeeId = employee.Id,
            UserName = account.UserName,
            Email = account.Email,
            IsActive = account.IsActive,
            CreatedAtUtc = account.CreatedAtUtc,
            RoleIds = new[] { role.Id }
        }, new[] { role.Code }, permissions);
    }

    private static bool IsValidEmail(string email)
    {
        try { return new MailAddress(email).Address.Equals(email, StringComparison.OrdinalIgnoreCase); }
        catch (FormatException) { return false; }
    }
}
