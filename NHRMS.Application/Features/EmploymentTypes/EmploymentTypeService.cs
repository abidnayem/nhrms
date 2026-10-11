using NHRMS.Application.Abstractions;
using NHRMS.Domain;

namespace NHRMS.Application.Features.EmploymentTypes;

public sealed class EmploymentTypeService
{
    private readonly IEmploymentTypeRepository _employmentTypeRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EmploymentTypeService(IEmploymentTypeRepository employmentTypeRepository, ICompanyRepository companyRepository, IUnitOfWork unitOfWork)
    {
        _employmentTypeRepository = employmentTypeRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<EmploymentTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var employmentTypes = await _employmentTypeRepository.GetAllAsync(cancellationToken);
        return employmentTypes.Select(Map).ToList();
    }

    public async Task<EmploymentTypeDto> CreateAsync(CreateEmploymentTypeRequest request, CancellationToken cancellationToken = default)
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

        var name = request.Name.Trim();
        var code = request.Code.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Employment type name is required.", nameof(request.Name));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Employment type code is required.", nameof(request.Code));
        }

        if (await _employmentTypeRepository.GetByCodeAsync(request.CompanyId, code, cancellationToken) is not null)
        {
            throw new InvalidOperationException($"Employment type code '{code}' already exists.");
        }

        var employmentType = new EmploymentType
        {
            CompanyId = company.Id,
            Name = name,
            Code = code,
            IsActive = request.IsActive,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await _employmentTypeRepository.AddAsync(employmentType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(employmentType);
    }

    private static EmploymentTypeDto Map(EmploymentType employmentType)
        => new()
        {
            Id = employmentType.Id,
            CompanyId = employmentType.CompanyId,
            Name = employmentType.Name,
            Code = employmentType.Code,
            IsActive = employmentType.IsActive,
            CreatedAtUtc = employmentType.CreatedAtUtc
        };
}
