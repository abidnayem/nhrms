using NHRMS.Application.Abstractions;
using NHRMS.Domain;

namespace NHRMS.Application.Features.Departments;

public sealed class DepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentService(
        IDepartmentRepository departmentRepository,
        ICompanyRepository companyRepository,
        IBranchRepository branchRepository,
        IUnitOfWork unitOfWork)
    {
        _departmentRepository = departmentRepository;
        _companyRepository = companyRepository;
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<DepartmentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var departments = await _departmentRepository.GetAllAsync(cancellationToken);
        return departments.Select(Map).ToList();
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default)
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

        if (request.BranchId.HasValue && request.BranchId.Value != Guid.Empty)
        {
            var branch = await _branchRepository.GetByIdAsync(request.BranchId.Value, cancellationToken);
            if (branch is null || branch.CompanyId != request.CompanyId)
            {
                throw new InvalidOperationException("Branch not found for the selected company.");
            }
        }

        if (request.ParentDepartmentId.HasValue && request.ParentDepartmentId.Value != Guid.Empty)
        {
            var parent = await _departmentRepository.GetByIdAsync(request.ParentDepartmentId.Value, cancellationToken);
            if (parent is null || parent.CompanyId != request.CompanyId)
            {
                throw new InvalidOperationException("Parent department not found for the selected company.");
            }
        }

        var name = request.Name.Trim();
        var code = request.Code.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Department name is required.", nameof(request.Name));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Department code is required.", nameof(request.Code));
        }

        if (await _departmentRepository.GetByCodeAsync(request.CompanyId, code, cancellationToken) is not null)
        {
            throw new InvalidOperationException($"Department code '{code}' already exists.");
        }

        var department = new Department
        {
            CompanyId = company.Id,
            BranchId = request.BranchId,
            ParentDepartmentId = request.ParentDepartmentId,
            Name = name,
            Code = code,
            IsActive = request.IsActive,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await _departmentRepository.AddAsync(department, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(department);
    }

    private static DepartmentDto Map(Department department)
        => new()
        {
            Id = department.Id,
            CompanyId = department.CompanyId,
            BranchId = department.BranchId,
            ParentDepartmentId = department.ParentDepartmentId,
            Name = department.Name,
            Code = department.Code,
            IsActive = department.IsActive,
            CreatedAtUtc = department.CreatedAtUtc
        };
}
