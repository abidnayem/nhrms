using NHRMS.Application.Abstractions;
using NHRMS.Domain;

namespace NHRMS.Application.Features.Employees;

public sealed class EmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EmployeeService(IEmployeeRepository employeeRepository, ICompanyRepository companyRepository, IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);
        return employees.Select(Map).ToList();
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
        if (company is null)
        {
            throw new InvalidOperationException("Company not found.");
        }

        var employeeCode = request.EmployeeCode.Trim();
        if (string.IsNullOrWhiteSpace(employeeCode))
        {
            throw new ArgumentException("Employee code is required.", nameof(request.EmployeeCode));
        }

        if (await _employeeRepository.GetByEmployeeCodeAsync(employeeCode, cancellationToken) is not null)
        {
            throw new InvalidOperationException($"Employee code '{employeeCode}' already exists.");
        }

        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("First name and last name are required.", nameof(request.FirstName));
        }

        if (request.DateOfLeaving.HasValue && request.DateOfJoining > request.DateOfLeaving.Value)
        {
            throw new ArgumentException("Joining date cannot be later than leaving date.");
        }

        var employee = new Employee
        {
            CompanyId = company.Id,
            EmployeeCode = employeeCode,
            FirstName = firstName,
            LastName = lastName,
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            DateOfJoining = request.DateOfJoining,
            DateOfLeaving = request.DateOfLeaving,
            DepartmentId = request.DepartmentId,
            DesignationId = request.DesignationId,
            EmploymentTypeId = request.EmploymentTypeId,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await _employeeRepository.AddAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(employee);
    }

    private static EmployeeDto Map(Employee employee)
        => new()
        {
            Id = employee.Id,
            CompanyId = employee.CompanyId,
            EmployeeCode = employee.EmployeeCode,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            DateOfJoining = employee.DateOfJoining,
            DateOfLeaving = employee.DateOfLeaving,
            IsActive = employee.IsActive
        };
}
