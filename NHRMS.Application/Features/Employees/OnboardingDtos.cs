using NHRMS.Domain;

namespace NHRMS.Application.Features.Employees;

public sealed class CreateOnboardingTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsRequired { get; set; } = true;
}

public sealed class UpdateOnboardingTaskStatusRequest
{
    public OnboardingTaskStatus Status { get; set; }
}

public sealed class OnboardingTaskDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsRequired { get; set; }
    public OnboardingTaskStatus Status { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}
