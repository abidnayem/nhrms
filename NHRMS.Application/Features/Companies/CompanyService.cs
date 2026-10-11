using NHRMS.Application.Abstractions;
using NHRMS.Domain;

namespace NHRMS.Application.Features.Companies;

public sealed class CompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompanyService(ICompanyRepository companyRepository, IUnitOfWork unitOfWork)
    {
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CompanyDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var companies = await _companyRepository.GetAllAsync(cancellationToken);
        return companies.Select(Map).ToList();
    }

    public async Task<CompanyDto> CreateAsync(CreateCompanyRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var name = request.Name.Trim();
        var code = request.Code.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Company name is required.", nameof(request.Name));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Company code is required.", nameof(request.Code));
        }

        if (await _companyRepository.GetByCodeAsync(code, cancellationToken) is not null)
        {
            throw new InvalidOperationException($"Company code '{code}' already exists.");
        }

        var company = new Company
        {
            Name = name,
            Code = code,
            TimeZoneId = string.IsNullOrWhiteSpace(request.TimeZoneId) ? null : request.TimeZoneId.Trim(),
            IsActive = request.IsActive,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await _companyRepository.AddAsync(company, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(company);
    }

    private static CompanyDto Map(Company company)
        => new()
        {
            Id = company.Id,
            Name = company.Name,
            Code = company.Code,
            TimeZoneId = company.TimeZoneId,
            IsActive = company.IsActive,
            CreatedAtUtc = company.CreatedAtUtc
        };
}
