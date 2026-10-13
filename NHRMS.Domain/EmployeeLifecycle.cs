namespace NHRMS.Domain;

public enum OnboardingTaskStatus
{
    Pending,
    Completed,
    Waived
}

public class EmployeeOnboardingTask : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsRequired { get; set; } = true;
    public OnboardingTaskStatus Status { get; set; } = OnboardingTaskStatus.Pending;
    public DateTime? CompletedAtUtc { get; set; }
}

public enum ProbationCaseStatus
{
    InProgress,
    Extended,
    Confirmed
}

public enum ProbationRecommendation
{
    Continue,
    Confirm,
    Extend
}

public enum ProbationReviewStatus
{
    Pending,
    Approved,
    Rejected
}

public class ProbationCase : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? ExtendedEndDate { get; set; }
    public ProbationCaseStatus Status { get; set; } = ProbationCaseStatus.InProgress;
    public DateTime? ConfirmedAtUtc { get; set; }
    public ICollection<ProbationReview> Reviews { get; private set; } = new HashSet<ProbationReview>();
}

public class ProbationReview : AuditableEntity
{
    public Guid ProbationCaseId { get; set; }
    public ProbationCase? ProbationCase { get; set; }
    public ProbationRecommendation Recommendation { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime? ProposedEndDate { get; set; }
    public ProbationReviewStatus Status { get; set; } = ProbationReviewStatus.Pending;
    public string ReviewedBy { get; set; } = string.Empty;
    public DateTime ReviewedAtUtc { get; set; }
    public string? DecidedBy { get; set; }
    public DateTime? DecidedAtUtc { get; set; }
    public string? DecisionNotes { get; set; }
}

public enum EmploymentChangeType
{
    CareerChange,
    Transfer,
    EmploymentTypeChange
}

public enum EmploymentChangeStatus
{
    Pending,
    Approved,
    Rejected,
    Applied
}

public class EmploymentChange : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public EmploymentChangeType ChangeType { get; set; }
    public EmploymentChangeStatus Status { get; set; } = EmploymentChangeStatus.Pending;
    public DateTime EffectiveDate { get; set; }
    public Guid? PreviousDepartmentId { get; set; }
    public Guid? NewDepartmentId { get; set; }
    public Guid? PreviousDesignationId { get; set; }
    public Guid? NewDesignationId { get; set; }
    public Guid? PreviousEmploymentTypeId { get; set; }
    public Guid? NewEmploymentTypeId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public DateTime RequestedAtUtc { get; set; }
    public string? DecidedBy { get; set; }
    public DateTime? DecidedAtUtc { get; set; }
    public string? DecisionNotes { get; set; }
    public DateTime? AppliedAtUtc { get; set; }
}

public enum DisciplinaryCaseType
{
    ShowCause,
    Warning,
    Investigation,
    Other
}

public enum DisciplinaryCaseStatus
{
    Open,
    Resolved,
    Dismissed
}

public class DisciplinaryCase : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DisciplinaryCaseType CaseType { get; set; }
    public DisciplinaryCaseStatus Status { get; set; } = DisciplinaryCaseStatus.Open;
    public DateTime OccurredOn { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string OpenedBy { get; set; } = string.Empty;
    public DateTime OpenedAtUtc { get; set; }
    public string? Decision { get; set; }
    public string? DecidedBy { get; set; }
    public DateTime? DecidedAtUtc { get; set; }
}
