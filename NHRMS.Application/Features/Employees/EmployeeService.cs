using NHRMS.Application.Abstractions;
using NHRMS.Domain;
using System.Net.Mail;

namespace NHRMS.Application.Features.Employees;

public sealed class EmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IDesignationRepository _designationRepository;
    private readonly IEmploymentTypeRepository _employmentTypeRepository;
    private readonly ICurrentActor _currentActor;
    private readonly IUnitOfWork _unitOfWork;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        ICompanyRepository companyRepository,
        IDepartmentRepository departmentRepository,
        IDesignationRepository designationRepository,
        IEmploymentTypeRepository employmentTypeRepository,
        ICurrentActor currentActor,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _companyRepository = companyRepository;
        _departmentRepository = departmentRepository;
        _designationRepository = designationRepository;
        _employmentTypeRepository = employmentTypeRepository;
        _currentActor = currentActor;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);
        return employees.Select(Map).ToList();
    }

    public async Task<EmployeeDirectoryDto> SearchAsync(
        EmployeeDirectoryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.Page < 1)
            throw new ArgumentException("Page must be at least 1.", nameof(query.Page));
        if (query.PageSize < 1 || query.PageSize > 100)
            throw new ArgumentException("Page size must be between 1 and 100.", nameof(query.PageSize));
        if (query.Page > int.MaxValue / query.PageSize)
            throw new ArgumentException("Requested page is outside the supported range.", nameof(query.Page));

        var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();
        var skip = checked((query.Page - 1) * query.PageSize);
        var (employees, totalCount) = await _employeeRepository.SearchAsync(
            search,
            query.CompanyId,
            query.DepartmentId,
            query.IsActive,
            skip,
            query.PageSize,
            cancellationToken);

        return new EmployeeDirectoryDto
        {
            Items = employees.Select(Map).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.CompanyId == Guid.Empty)
        {
            throw new ArgumentException("Company is required.", nameof(request.CompanyId));
        }

        var company = await _companyRepository.GetByIdAsync(request.CompanyId, cancellationToken);
        if (company is null || !company.IsActive)
        {
            throw new InvalidOperationException("An active company is required.");
        }

        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("First name and last name are required.", nameof(request.FirstName));
        }

        if (request.DateOfJoining == default)
        {
            throw new ArgumentException("Joining date is required.", nameof(request.DateOfJoining));
        }

        if (request.DateOfLeaving.HasValue && request.DateOfJoining > request.DateOfLeaving.Value)
        {
            throw new ArgumentException("Joining date cannot be later than leaving date.");
        }

        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        if (email is not null && !IsValidEmail(email))
        {
            throw new ArgumentException("A valid email address is required.", nameof(request.Email));
        }

        await EnsureCompanyReferenceAsync(
            request.DepartmentId,
            _departmentRepository.GetByIdAsync,
            x => x.CompanyId,
            x => x.IsActive,
            "Department",
            company.Id,
            cancellationToken);
        await EnsureCompanyReferenceAsync(
            request.DesignationId,
            _designationRepository.GetByIdAsync,
            x => x.CompanyId,
            x => x.IsActive,
            "Designation",
            company.Id,
            cancellationToken);
        await EnsureCompanyReferenceAsync(
            request.EmploymentTypeId,
            _employmentTypeRepository.GetByIdAsync,
            x => x.CompanyId,
            x => x.IsActive,
            "Employment type",
            company.Id,
            cancellationToken);

        var employee = new Employee
        {
            CompanyId = company.Id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            DateOfJoining = request.DateOfJoining,
            DateOfLeaving = request.DateOfLeaving,
            DepartmentId = request.DepartmentId,
            DesignationId = request.DesignationId,
            EmploymentTypeId = request.EmploymentTypeId,
            IsActive = !request.DateOfLeaving.HasValue,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = _currentActor.Name
        };
        var employeeCodeNumber = await _employeeRepository.GetNextEmployeeCodeNumberAsync(cancellationToken);
        employee.EmployeeCode = $"EMP-{employeeCodeNumber:D6}";

        await _employeeRepository.AddAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(employee);
    }

    private static EmployeeDto Map(Employee employee)
        => new()
        {
            Id = employee.Id,
            CompanyId = employee.CompanyId,
            DepartmentId = employee.DepartmentId,
            DesignationId = employee.DesignationId,
            EmploymentTypeId = employee.EmploymentTypeId,
            EmployeeCode = employee.EmployeeCode,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            DateOfJoining = employee.DateOfJoining,
            DateOfLeaving = employee.DateOfLeaving,
            IsActive = employee.IsActive
        };

    private static bool IsValidEmail(string email)
    {
        try
        {
            return new MailAddress(email).Address.Equals(email, StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static async Task EnsureCompanyReferenceAsync<TEntity>(
        Guid? id,
        Func<Guid, CancellationToken, Task<TEntity?>> getById,
        Func<TEntity, Guid> getCompanyId,
        Func<TEntity, bool> isActive,
        string referenceName,
        Guid companyId,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        if (!id.HasValue)
            return;

        var reference = await getById(id.Value, cancellationToken);
        if (reference is null || !isActive(reference) || getCompanyId(reference) != companyId)
        {
            throw new ArgumentException($"{referenceName} must be active and belong to the selected company.", referenceName);
        }
    }
}
