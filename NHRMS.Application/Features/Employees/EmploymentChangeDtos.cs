using NHRMS.Domain;

namespace NHRMS.Application.Features.Employees;

public sealed class RequestEmploymentChange
{
    public Guid? DepartmentId { get; init; }
    public Guid? DesignationId { get; init; }
    public Guid? EmploymentTypeId { get; init; }
    public DateTime EffectiveDate { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public sealed class DecideEmploymentChange
{
    public bool Approve { get; init; }
    public string? Notes { get; init; }
}

public sealed class EmploymentChangeDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public EmploymentChangeType ChangeType { get; init; }
    public EmploymentChangeStatus Status { get; init; }
    public DateTime EffectiveDate { get; init; }
    public Guid? PreviousDepartmentId { get; init; }
    public Guid? NewDepartmentId { get; init; }
    public Guid? PreviousDesignationId { get; init; }
    public Guid? NewDesignationId { get; init; }
    public Guid? PreviousEmploymentTypeId { get; init; }
    public Guid? NewEmploymentTypeId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string RequestedBy { get; init; } = string.Empty;
    public DateTime RequestedAtUtc { get; init; }
    public string? DecidedBy { get; init; }
    public DateTime? DecidedAtUtc { get; init; }
    public string? DecisionNotes { get; init; }
    public DateTime? AppliedAtUtc { get; init; }
}
