using NHRMS.Application.Abstractions;
using NHRMS.Domain;

namespace NHRMS.Application.Features.Employees;

public sealed class DisciplinaryService
{
    private readonly IEmployeeRepository _employees;
    private readonly IDisciplinaryCaseRepository _cases;
    private readonly ICurrentActor _actor;
    private readonly IUnitOfWork _unitOfWork;

    public DisciplinaryService(IEmployeeRepository employees, IDisciplinaryCaseRepository cases, ICurrentActor actor, IUnitOfWork unitOfWork)
    { _employees = employees; _cases = cases; _actor = actor; _unitOfWork = unitOfWork; }

    public async Task<IReadOnlyList<DisciplinaryCaseDto>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default)
    {
        if (await _employees.GetByIdAsync(employeeId, ct) is null)
            throw new KeyNotFoundException("Employee was not found.");
        return (await _cases.GetByEmployeeIdAsync(employeeId, ct)).Select(Map).ToList();
    }

    public async Task<DisciplinaryCaseDto> CreateAsync(Guid employeeId, CreateDisciplinaryCaseRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var employee = await _employees.GetByIdAsync(employeeId, ct) ?? throw new KeyNotFoundException("Employee was not found.");
        if (request.OccurredOn == default || request.OccurredOn.Date > DateTime.UtcNow.Date || request.OccurredOn.Date < employee.DateOfJoining.Date ||
            (employee.DateOfLeaving.HasValue && request.OccurredOn.Date > employee.DateOfLeaving.Value.Date))
            throw new ArgumentException("The incident date must fall within the employee's employment dates.", nameof(request.OccurredOn));
        if (!Enum.IsDefined(request.CaseType)) throw new ArgumentException("A valid case type is required.", nameof(request.CaseType));
        var subject = request.Subject.Trim();
        var summary = request.Summary.Trim();
        if (subject.Length is < 3 or > 200) throw new ArgumentException("Subject must contain between 3 and 200 characters.", nameof(request.Subject));
        if (summary.Length is < 3 or > 4000) throw new ArgumentException("Summary must contain between 3 and 4000 characters.", nameof(request.Summary));
        var now = DateTime.UtcNow;
        var item = new DisciplinaryCase
        {
            EmployeeId = employeeId, CaseType = request.CaseType, OccurredOn = request.OccurredOn.Date,
            Subject = subject, Summary = summary, Status = DisciplinaryCaseStatus.Open,
            OpenedBy = _actor.Name, OpenedAtUtc = now, CreatedBy = _actor.Name
        };
        await _cases.AddAsync(item, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Map(item);
    }

    public async Task<DisciplinaryCaseDto> DecideAsync(Guid employeeId, Guid caseId, DecideDisciplinaryCaseRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var item = await _cases.GetCaseAsync(employeeId, caseId, ct) ?? throw new KeyNotFoundException("Disciplinary case was not found.");
        if (item.Status != DisciplinaryCaseStatus.Open) throw new InvalidOperationException("Only open cases can receive a decision.");
        if (string.Equals(item.OpenedBy, _actor.Name, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The case opener cannot decide the case.");
        var decision = request.Decision.Trim();
        if (decision.Length is < 3 or > 4000) throw new ArgumentException("A decision between 3 and 4000 characters is required.", nameof(request.Decision));
        item.Status = request.Uphold ? DisciplinaryCaseStatus.Resolved : DisciplinaryCaseStatus.Dismissed;
        item.Decision = decision;
        item.DecidedBy = _actor.Name;
        item.DecidedAtUtc = DateTime.UtcNow;
        item.UpdatedBy = _actor.Name;
        item.UpdatedAtUtc = item.DecidedAtUtc;
        await _cases.UpdateAsync(item, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Map(item);
    }

    private static DisciplinaryCaseDto Map(DisciplinaryCase x) => new()
    {
        Id = x.Id, EmployeeId = x.EmployeeId, CaseType = x.CaseType, Status = x.Status,
        OccurredOn = x.OccurredOn, Subject = x.Subject, Summary = x.Summary, OpenedBy = x.OpenedBy,
        OpenedAtUtc = x.OpenedAtUtc, Decision = x.Decision, DecidedBy = x.DecidedBy, DecidedAtUtc = x.DecidedAtUtc
    };
}
