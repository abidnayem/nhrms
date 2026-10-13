using NHRMS.Domain;

namespace NHRMS.Application.Features.Employees;

public sealed class CreateDisciplinaryCaseRequest
{
    public DisciplinaryCaseType CaseType { get; init; }
    public DateTime OccurredOn { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
}

public sealed class DecideDisciplinaryCaseRequest
{
    public bool Uphold { get; init; }
    public string Decision { get; init; } = string.Empty;
}

public sealed class DisciplinaryCaseDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public DisciplinaryCaseType CaseType { get; init; }
    public DisciplinaryCaseStatus Status { get; init; }
    public DateTime OccurredOn { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string OpenedBy { get; init; } = string.Empty;
    public DateTime OpenedAtUtc { get; init; }
    public string? Decision { get; init; }
    public string? DecidedBy { get; init; }
    public DateTime? DecidedAtUtc { get; init; }
}
