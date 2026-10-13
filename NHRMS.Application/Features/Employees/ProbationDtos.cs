using NHRMS.Domain;

namespace NHRMS.Application.Features.Employees;

public sealed class StartProbationRequest
{
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
}

public sealed class CreateProbationReviewRequest
{
    public ProbationRecommendation Recommendation { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime? ProposedEndDate { get; set; }
}

public sealed class DecideProbationReviewRequest
{
    public bool Approve { get; set; }
    public string? Notes { get; set; }
}

public sealed class ProbationReviewDto
{
    public Guid Id { get; set; }
    public ProbationRecommendation Recommendation { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime? ProposedEndDate { get; set; }
    public ProbationReviewStatus Status { get; set; }
    public string ReviewedBy { get; set; } = string.Empty;
    public DateTime ReviewedAtUtc { get; set; }
    public string? DecidedBy { get; set; }
    public DateTime? DecidedAtUtc { get; set; }
    public string? DecisionNotes { get; set; }
}

public sealed class ProbationCaseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? ExtendedEndDate { get; set; }
    public DateTime CurrentEndDate { get; set; }
    public ProbationCaseStatus Status { get; set; }
    public DateTime? ConfirmedAtUtc { get; set; }
    public IReadOnlyList<ProbationReviewDto> Reviews { get; set; } = Array.Empty<ProbationReviewDto>();
}
