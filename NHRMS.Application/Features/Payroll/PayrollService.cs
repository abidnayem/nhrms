using NHRMS.Application.Abstractions;
using NHRMS.Domain;

namespace NHRMS.Application.Features.Payroll;

public sealed class PayrollCycleService
{
    private readonly IPayrollCycleRepository _payrollCycleRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PayrollCycleService(IPayrollCycleRepository payrollCycleRepository, ICompanyRepository companyRepository, IUnitOfWork unitOfWork)
    {
        _payrollCycleRepository = payrollCycleRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<PayrollCycleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cycles = await _payrollCycleRepository.GetAllAsync(cancellationToken);
        return cycles.Select(Map).ToList();
    }

    public async Task<PayrollCycleDto> CreateAsync(CreatePayrollCycleRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (request.CompanyId == Guid.Empty)
            throw new ArgumentException("Company is required.", nameof(request.CompanyId));

        var company = await _companyRepository.GetByIdAsync(request.CompanyId, cancellationToken);
        if (company is null)
            throw new InvalidOperationException("Company not found.");

        if (request.PeriodStart >= request.PeriodEnd)
            throw new ArgumentException("Period end must be later than period start.");

        if (request.TotalGross < 0 || request.TotalDeductions < 0 || request.TotalDeductions > request.TotalGross)
            throw new ArgumentException("Payroll amounts must be non-negative and deductions cannot exceed gross pay.");

        var net = request.TotalGross - request.TotalDeductions;

        var cycle = new PayrollCycle
        {
            CompanyId = company.Id,
            PeriodStart = request.PeriodStart.Date,
            PeriodEnd = request.PeriodEnd.Date,
            RunDateUtc = DateTime.UtcNow,
            TotalGross = request.TotalGross,
            TotalDeductions = request.TotalDeductions,
            TotalNet = net,
            Status = "Draft",
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await _payrollCycleRepository.AddAsync(cycle, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(cycle);
    }

    private static PayrollCycleDto Map(PayrollCycle payrollCycle)
        => new()
        {
            Id = payrollCycle.Id,
            CompanyId = payrollCycle.CompanyId,
            PeriodStart = payrollCycle.PeriodStart,
            PeriodEnd = payrollCycle.PeriodEnd,
            TotalGross = payrollCycle.TotalGross,
            TotalDeductions = payrollCycle.TotalDeductions,
            TotalNet = payrollCycle.TotalNet,
            Status = payrollCycle.Status,
            RunDateUtc = payrollCycle.RunDateUtc
        };
}
