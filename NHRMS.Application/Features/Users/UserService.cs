using System.Security.Cryptography;
using NHRMS.Application.Abstractions;
using NHRMS.Domain;

namespace NHRMS.Application.Features.Users;

public sealed class UserAccountService
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IUserRoleRepository? _userRoleRepository;
    private readonly IRoleRepository? _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserAccountService(IUserAccountRepository userAccountRepository, IUnitOfWork unitOfWork)
        : this(userAccountRepository, null, null, unitOfWork)
    {
    }

    public UserAccountService(
        IUserAccountRepository userAccountRepository,
        IUserRoleRepository? userRoleRepository,
        IRoleRepository? roleRepository,
        IUnitOfWork unitOfWork)
    {
        _userAccountRepository = userAccountRepository;
        _userRoleRepository = userRoleRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<UserAccountDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userAccountRepository.GetAllAsync(cancellationToken);
        var dtos = new List<UserAccountDto>();

        foreach (var user in users)
        {
            var roleIds = _userRoleRepository is null
                ? Array.Empty<Guid>()
                : (await _userRoleRepository.GetByUserIdAsync(user.Id, cancellationToken)).Select(x => x.RoleId).ToArray();

            dtos.Add(Map(user, roleIds));
        }

        return dtos;
    }

    public async Task<UserAccountDto> CreateAsync(CreateUserAccountRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.EmployeeId == Guid.Empty)
        {
            throw new ArgumentException("Employee is required.", nameof(request.EmployeeId));
        }

        var userName = request.UserName.Trim();
        var email = request.Email.Trim();

        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("User name is required.", nameof(request.UserName));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(request.Email));
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            throw new ArgumentException("Password must be at least 8 characters long.", nameof(request.Password));
        }

        if (await _userAccountRepository.GetByUsernameAsync(userName, cancellationToken) is not null)
        {
            throw new InvalidOperationException($"User name '{userName}' already exists.");
        }

        if (await _userAccountRepository.GetByEmailAsync(email, cancellationToken) is not null)
        {
            throw new InvalidOperationException($"Email '{email}' already exists.");
        }

        if (await _userAccountRepository.GetByEmployeeIdAsync(request.EmployeeId, cancellationToken) is not null)
        {
            throw new InvalidOperationException("An account is already assigned to this employee.");
        }

        var account = new UserAccount
        {
            EmployeeId = request.EmployeeId,
            UserName = userName,
            Email = email,
            PasswordHash = PasswordHashing.Hash(request.Password),
            IsActive = request.IsActive,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await _userAccountRepository.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(account, Array.Empty<Guid>());
    }

    public async Task<AuthenticatedUserDto?> AuthenticateAsync(
        string userName,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrEmpty(password))
            return null;
        var account = await _userAccountRepository.GetByUsernameAsync(userName.Trim(), cancellationToken);
        if (account is null || !account.IsActive || !PasswordHashing.Verify(password, account.PasswordHash, out var needsUpgrade))
            return null;

        if (needsUpgrade)
        {
            account.PasswordHash = PasswordHashing.Hash(password);
            await _userAccountRepository.UpdateAsync(account, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var roles = Array.Empty<string>();
        if (_userRoleRepository is not null && _roleRepository is not null)
        {
            var assignments = await _userRoleRepository.GetByUserIdAsync(account.Id, cancellationToken);
            var activeRoleIds = assignments.Where(x => x.IsActive).Select(x => x.RoleId).ToHashSet();
            roles = (await _roleRepository.GetAllAsync(cancellationToken))
                .Where(role => role.IsActive && activeRoleIds.Contains(role.Id))
                .Select(role => role.Code)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        var permissions = _userRoleRepository is null
            ? Array.Empty<string>()
            : await _userRoleRepository.GetPermissionCodesByUserIdAsync(account.Id, cancellationToken);
        return new AuthenticatedUserDto(Map(account, Array.Empty<Guid>()), roles, permissions);
    }

    public async Task<UserAccountDto> AssignRoleAsync(AssignRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException("User is required.", nameof(request.UserId));
        }

        if (request.RoleId == Guid.Empty)
        {
            throw new ArgumentException("Role is required.", nameof(request.RoleId));
        }

        if (_userRoleRepository is null || _roleRepository is null)
        {
            throw new InvalidOperationException("Role assignment is not available in this service configuration.");
        }

        var user = await _userAccountRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            throw new InvalidOperationException("Role not found.");
        }

        if (await _userRoleRepository.GetByUserAndRoleAsync(request.UserId, request.RoleId, cancellationToken) is not null)
        {
            throw new InvalidOperationException("Role is already assigned to this user.");
        }

        var assignment = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await _userRoleRepository.AddAsync(assignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = await _userRoleRepository.GetByUserIdAsync(user.Id, cancellationToken);
        return Map(user, roles.Select(x => x.RoleId).ToArray());
    }

    private static UserAccountDto Map(UserAccount account, IEnumerable<Guid>? roleIds)
        => new()
        {
            Id = account.Id,
            EmployeeId = account.EmployeeId,
            UserName = account.UserName,
            Email = account.Email,
            IsActive = account.IsActive,
            CreatedAtUtc = account.CreatedAtUtc,
            RoleIds = roleIds?.OrderBy(x => x).ToArray() ?? Array.Empty<Guid>()
        };
}
