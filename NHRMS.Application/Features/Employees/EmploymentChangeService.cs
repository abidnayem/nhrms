using NHRMS.Application.Abstractions;
using NHRMS.Domain;

namespace NHRMS.Application.Features.Employees;

public sealed class EmploymentChangeService
{
    private readonly IEmployeeRepository _employees;
    private readonly IDepartmentRepository _departments;
    private readonly IDesignationRepository _designations;
    private readonly IEmploymentTypeRepository _employmentTypes;
    private readonly IEmploymentChangeRepository _changes;
    private readonly ICurrentActor _actor;
    private readonly IUnitOfWork _unitOfWork;

    public EmploymentChangeService(IEmployeeRepository employees, IDepartmentRepository departments,
        IDesignationRepository designations, IEmploymentTypeRepository employmentTypes,
        IEmploymentChangeRepository changes, ICurrentActor actor, IUnitOfWork unitOfWork)
    {
        _employees = employees;
        _departments = departments;
        _designations = designations;
        _employmentTypes = employmentTypes;
        _changes = changes;
        _actor = actor;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<EmploymentChangeDto>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default)
    {
        if (await _employees.GetByIdAsync(employeeId, ct) is null)
            throw new KeyNotFoundException("Employee was not found.");
        return (await _changes.GetByEmployeeIdAsync(employeeId, ct)).Select(Map).ToList();
    }

    public async Task<EmploymentChangeDto> RequestAsync(Guid employeeId, RequestEmploymentChange request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var employee = await _employees.GetByIdAsync(employeeId, ct)
            ?? throw new KeyNotFoundException("Employee was not found.");
        if (!employee.IsActive)
            throw new InvalidOperationException("Employment changes can only be requested for active employees.");
        if (request.EffectiveDate.Date < DateTime.UtcNow.Date)
            throw new ArgumentException("Effective date cannot be in the past.", nameof(request.EffectiveDate));
        var reason = request.Reason.Trim();
        if (reason.Length is < 3 or > 2000)
            throw new ArgumentException("Reason must contain between 3 and 2000 characters.", nameof(request.Reason));
        if (await _changes.HasOpenChangeAsync(employeeId, ct))
            throw new InvalidOperationException("This employee already has a pending or approved effective change.");

        await ValidateReferenceAsync(request.DepartmentId, employee.CompanyId, _departments.GetByIdAsync,
            x => x.CompanyId, x => x.IsActive, "Department", ct);
        await ValidateReferenceAsync(request.DesignationId, employee.CompanyId, _designations.GetByIdAsync,
            x => x.CompanyId, x => x.IsActive, "Designation", ct);
        await ValidateReferenceAsync(request.EmploymentTypeId, employee.CompanyId, _employmentTypes.GetByIdAsync,
            x => x.CompanyId, x => x.IsActive, "Employment type", ct);

        var departmentId = request.DepartmentId ?? employee.DepartmentId;
        var designationId = request.DesignationId ?? employee.DesignationId;
        var employmentTypeId = request.EmploymentTypeId ?? employee.EmploymentTypeId;
        if (departmentId == employee.DepartmentId && designationId == employee.DesignationId && employmentTypeId == employee.EmploymentTypeId)
            throw new ArgumentException("At least one employment assignment must change.");

        var changeType = departmentId != employee.DepartmentId
            ? EmploymentChangeType.Transfer
            : employmentTypeId != employee.EmploymentTypeId
                ? EmploymentChangeType.EmploymentTypeChange
                : EmploymentChangeType.CareerChange;
        var change = new EmploymentChange
        {
            EmployeeId = employeeId,
            ChangeType = changeType,
            EffectiveDate = request.EffectiveDate.Date,
            PreviousDepartmentId = employee.DepartmentId,
            NewDepartmentId = departmentId,
            PreviousDesignationId = employee.DesignationId,
            NewDesignationId = designationId,
            PreviousEmploymentTypeId = employee.EmploymentTypeId,
            NewEmploymentTypeId = employmentTypeId,
            Reason = reason,
            RequestedBy = _actor.Name,
            RequestedAtUtc = DateTime.UtcNow,
            Status = EmploymentChangeStatus.Pending,
            CreatedBy = _actor.Name
        };
        await _changes.AddAsync(change, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Map(change);
    }

    public async Task<EmploymentChangeDto> DecideAsync(Guid employeeId, Guid changeId, DecideEmploymentChange request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var change = await _changes.GetChangeAsync(employeeId, changeId, ct)
            ?? throw new KeyNotFoundException("Employment change was not found.");
        if (change.Status != EmploymentChangeStatus.Pending)
            throw new InvalidOperationException("Only pending changes can be decided.");
        if (string.Equals(change.RequestedBy, _actor.Name, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("A requester cannot approve or reject their own employment change.");
        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        if (notes?.Length > 2000)
            throw new ArgumentException("Decision notes cannot exceed 2000 characters.", nameof(request.Notes));
        if (!request.Approve && string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException("A reason is required when rejecting a change.", nameof(request.Notes));

        change.Status = request.Approve ? EmploymentChangeStatus.Approved : EmploymentChangeStatus.Rejected;
        change.DecidedBy = _actor.Name;
        change.DecidedAtUtc = DateTime.UtcNow;
        change.DecisionNotes = notes;
        change.UpdatedBy = _actor.Name;
        change.UpdatedAtUtc = DateTime.UtcNow;
        if (request.Approve && change.EffectiveDate.Date <= DateTime.UtcNow.Date)
            await ApplyAsync(change, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Map(change);
    }

    public async Task<int> ApplyDueAsync(Guid employeeId, CancellationToken ct = default)
    {
        if (await _employees.GetByIdAsync(employeeId, ct) is null)
            throw new KeyNotFoundException("Employee was not found.");
        var due = await _changes.GetDueApprovedAsync(employeeId, DateTime.UtcNow.Date, ct);
        foreach (var change in due)
            await ApplyAsync(change, ct);
        if (due.Count > 0)
            await _unitOfWork.SaveChangesAsync(ct);
        return due.Count;
    }

    private async Task ApplyAsync(EmploymentChange change, CancellationToken ct)
    {
        var employee = await _employees.GetByIdAsync(change.EmployeeId, ct)
            ?? throw new InvalidOperationException("Employee for an approved change no longer exists.");
        if (employee.DepartmentId != change.PreviousDepartmentId || employee.DesignationId != change.PreviousDesignationId ||
            employee.EmploymentTypeId != change.PreviousEmploymentTypeId)
            throw new InvalidOperationException("The employee's assignments changed after this request was submitted; the approved change needs HR review.");
        employee.DepartmentId = change.NewDepartmentId;
        employee.DesignationId = change.NewDesignationId;
        employee.EmploymentTypeId = change.NewEmploymentTypeId;
        employee.UpdatedBy = _actor.Name;
        employee.UpdatedAtUtc = DateTime.UtcNow;
        change.Status = EmploymentChangeStatus.Applied;
        change.AppliedAtUtc = DateTime.UtcNow;
        change.UpdatedBy = _actor.Name;
        change.UpdatedAtUtc = DateTime.UtcNow;
        await _employees.UpdateAsync(employee, ct);
        await _changes.UpdateAsync(change, ct);
    }

    private static async Task ValidateReferenceAsync<TEntity>(Guid? id, Guid companyId,
        Func<Guid, CancellationToken, Task<TEntity?>> getById, Func<TEntity, Guid> getCompanyId,
        Func<TEntity, bool> isActive, string label, CancellationToken ct) where TEntity : class
    {
        if (id is not { } value) return;
        var entity = await getById(value, ct);
        if (entity is null || !isActive(entity) || getCompanyId(entity) != companyId)
            throw new ArgumentException($"{label} must be active and belong to the employee's company.");
    }

    private static EmploymentChangeDto Map(EmploymentChange x) => new()
    {
        Id = x.Id, EmployeeId = x.EmployeeId, ChangeType = x.ChangeType, Status = x.Status,
        EffectiveDate = x.EffectiveDate, PreviousDepartmentId = x.PreviousDepartmentId, NewDepartmentId = x.NewDepartmentId,
        PreviousDesignationId = x.PreviousDesignationId, NewDesignationId = x.NewDesignationId,
        PreviousEmploymentTypeId = x.PreviousEmploymentTypeId, NewEmploymentTypeId = x.NewEmploymentTypeId,
        Reason = x.Reason, RequestedBy = x.RequestedBy, RequestedAtUtc = x.RequestedAtUtc,
        DecidedBy = x.DecidedBy, DecidedAtUtc = x.DecidedAtUtc, DecisionNotes = x.DecisionNotes, AppliedAtUtc = x.AppliedAtUtc
    };
}
