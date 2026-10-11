using NHRMS.Application.Abstractions;
using NHRMS.Domain;

namespace NHRMS.Application.Features.Designations;

public sealed class DesignationService
{
    private readonly IDesignationRepository _designationRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DesignationService(IDesignationRepository designationRepository, ICompanyRepository companyRepository, IUnitOfWork unitOfWork)
    {
        _designationRepository = designationRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<DesignationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var designations = await _designationRepository.GetAllAsync(cancellationToken);
        return designations.Select(Map).ToList();
    }

    public async Task<DesignationDto> CreateAsync(CreateDesignationRequest request, CancellationToken cancellationToken = default)
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
            throw new ArgumentException("Designation name is required.", nameof(request.Name));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Designation code is required.", nameof(request.Code));
        }

        if (await _designationRepository.GetByCodeAsync(request.CompanyId, code, cancellationToken) is not null)
        {
            throw new InvalidOperationException($"Designation code '{code}' already exists.");
        }

        var designation = new Designation
        {
            CompanyId = company.Id,
            Name = name,
            Code = code,
            IsActive = request.IsActive,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await _designationRepository.AddAsync(designation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(designation);
    }

    private static DesignationDto Map(Designation designation)
        => new()
        {
            Id = designation.Id,
            CompanyId = designation.CompanyId,
            Name = designation.Name,
            Code = designation.Code,
            IsActive = designation.IsActive,
            CreatedAtUtc = designation.CreatedAtUtc
        };
}
