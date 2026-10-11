using NHRMS.Application.Abstractions;
using NHRMS.Domain;

namespace NHRMS.Application.Features.Branches;

public sealed class BranchService
{
    private readonly IBranchRepository _branchRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BranchService(IBranchRepository branchRepository, ICompanyRepository companyRepository, IUnitOfWork unitOfWork)
    {
        _branchRepository = branchRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<BranchDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var branches = await _branchRepository.GetAllAsync(cancellationToken);
        return branches.Select(Map).ToList();
    }

    public async Task<BranchDto> CreateAsync(CreateBranchRequest request, CancellationToken cancellationToken = default)
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
            throw new ArgumentException("Branch name is required.", nameof(request.Name));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Branch code is required.", nameof(request.Code));
        }

        if (await _branchRepository.GetByCodeAsync(request.CompanyId, code, cancellationToken) is not null)
        {
            throw new InvalidOperationException($"Branch code '{code}' already exists.");
        }

        var branch = new Branch
        {
            CompanyId = company.Id,
            Name = name,
            Code = code,
            Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim(),
            IsActive = request.IsActive,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await _branchRepository.AddAsync(branch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(branch);
    }

    private static BranchDto Map(Branch branch)
        => new()
        {
            Id = branch.Id,
            CompanyId = branch.CompanyId,
            Name = branch.Name,
            Code = branch.Code,
            Address = branch.Address,
            IsActive = branch.IsActive,
            CreatedAtUtc = branch.CreatedAtUtc
        };
}
