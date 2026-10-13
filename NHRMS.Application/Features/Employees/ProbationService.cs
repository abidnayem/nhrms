using NHRMS.Application.Abstractions;
using NHRMS.Domain;

namespace NHRMS.Application.Features.Employees;

public sealed class ProbationService(
    IEmployeeRepository employeeRepository,
    IProbationRepository probationRepository,
    ICurrentActor currentActor,
    IUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<ProbationCaseDto>> GetCasesAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (await employeeRepository.GetByIdAsync(employeeId, cancellationToken) is null)
            throw new KeyNotFoundException("Employee not found.");
        var cases = await probationRepository.GetByEmployeeIdAsync(employeeId, cancellationToken);
        return cases.Select(Map).ToList();
    }

    public async Task<ProbationCaseDto> StartAsync(
        Guid employeeId,
        StartProbationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var employee = await employeeRepository.GetByIdAsync(employeeId, cancellationToken)
            ?? throw new KeyNotFoundException("Employee not found.");
        if (!employee.IsActive || employee.DateOfLeaving.HasValue)
            throw new InvalidOperationException("Probation can only be started for an active employee.");
        if (request.StartDate == default || request.StartDate.Date < employee.DateOfJoining.Date)
            throw new ArgumentException("Probation start date cannot be before the employee joining date.", nameof(request.StartDate));
        if (request.PlannedEndDate.Date <= request.StartDate.Date)
            throw new ArgumentException("Planned probation end date must be after its start date.", nameof(request.PlannedEndDate));

        var existing = await probationRepository.GetByEmployeeIdAsync(employeeId, cancellationToken);
        if (existing.Any(x => x.Status != ProbationCaseStatus.Confirmed))
            throw new InvalidOperationException("This employee already has an open probation case.");

        var probationCase = new ProbationCase
        {
            EmployeeId = employeeId,
            StartDate = request.StartDate.Date,
            PlannedEndDate = request.PlannedEndDate.Date,
            Status = ProbationCaseStatus.InProgress,
            CreatedBy = currentActor.Name
        };
        await probationRepository.AddAsync(probationCase, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(probationCase);
    }

    public async Task<ProbationCaseDto> RecommendAsync(
        Guid caseId,
        CreateProbationReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var probationCase = await probationRepository.GetWithReviewsAsync(caseId, cancellationToken)
            ?? throw new KeyNotFoundException("Probation case not found.");
        if (probationCase.Status == ProbationCaseStatus.Confirmed)
            throw new InvalidOperationException("A confirmed probation case cannot receive more reviews.");
        if (await probationRepository.HasPendingReviewAsync(caseId, cancellationToken))
            throw new InvalidOperationException("Resolve the pending probation review before creating another.");
        if (!Enum.IsDefined(request.Recommendation))
            throw new ArgumentException("A valid recommendation is required.", nameof(request.Recommendation));

        var reason = request.Reason.Trim();
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Review reason is required.", nameof(request.Reason));
        if (reason.Length > 2000)
            throw new ArgumentException("Review reason cannot exceed 2000 characters.", nameof(request.Reason));
        var currentEndDate = probationCase.ExtendedEndDate ?? probationCase.PlannedEndDate;
        if (request.Recommendation == ProbationRecommendation.Extend)
        {
            if (!request.ProposedEndDate.HasValue || request.ProposedEndDate.Value.Date <= currentEndDate.Date)
                throw new ArgumentException("An extension recommendation needs an end date after the current end date.", nameof(request.ProposedEndDate));
        }
        else if (request.ProposedEndDate.HasValue)
        {
            throw new ArgumentException("Only an extension recommendation can include a proposed end date.", nameof(request.ProposedEndDate));
        }

        var now = DateTime.UtcNow;
        var review = new ProbationReview
        {
            ProbationCaseId = caseId,
            Recommendation = request.Recommendation,
            Reason = reason,
            ProposedEndDate = request.ProposedEndDate?.Date,
            Status = ProbationReviewStatus.Pending,
            ReviewedBy = currentActor.Name,
            ReviewedAtUtc = now,
            CreatedBy = currentActor.Name
        };
        await probationRepository.AddReviewAsync(review, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(await probationRepository.GetWithReviewsAsync(caseId, cancellationToken) ?? probationCase);
    }

    public async Task<ProbationCaseDto> DecideAsync(
        Guid caseId,
        Guid reviewId,
        DecideProbationReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var probationCase = await probationRepository.GetByIdAsync(caseId, cancellationToken)
            ?? throw new KeyNotFoundException("Probation case not found.");
        var review = await probationRepository.GetReviewAsync(caseId, reviewId, cancellationToken)
            ?? throw new KeyNotFoundException("Probation review not found.");
        var actor = currentActor.Name;
        if (string.Equals(review.ReviewedBy, actor, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("A reviewer cannot approve or reject their own recommendation.");
        if (review.Status != ProbationReviewStatus.Pending)
            throw new InvalidOperationException("This probation review has already been decided.");

        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        if (notes?.Length > 2000)
            throw new ArgumentException("Decision notes cannot exceed 2000 characters.", nameof(request.Notes));
        var now = DateTime.UtcNow;
        review.Status = request.Approve ? ProbationReviewStatus.Approved : ProbationReviewStatus.Rejected;
        review.DecidedBy = actor;
        review.DecidedAtUtc = now;
        review.DecisionNotes = notes;
        review.UpdatedAtUtc = now;
        review.UpdatedBy = actor;

        if (request.Approve)
        {
            switch (review.Recommendation)
            {
                case ProbationRecommendation.Confirm:
                    probationCase.Status = ProbationCaseStatus.Confirmed;
                    probationCase.ConfirmedAtUtc = now;
                    break;
                case ProbationRecommendation.Extend:
                    probationCase.Status = ProbationCaseStatus.Extended;
                    probationCase.ExtendedEndDate = review.ProposedEndDate;
                    break;
            }
            probationCase.UpdatedAtUtc = now;
            probationCase.UpdatedBy = actor;
            await probationRepository.UpdateAsync(probationCase, cancellationToken);
        }

        await probationRepository.UpdateReviewAsync(review, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(await probationRepository.GetWithReviewsAsync(caseId, cancellationToken) ?? probationCase);
    }

    private static ProbationCaseDto Map(ProbationCase item) => new()
    {
        Id = item.Id,
        EmployeeId = item.EmployeeId,
        StartDate = item.StartDate,
        PlannedEndDate = item.PlannedEndDate,
        ExtendedEndDate = item.ExtendedEndDate,
        CurrentEndDate = item.ExtendedEndDate ?? item.PlannedEndDate,
        Status = item.Status,
        ConfirmedAtUtc = item.ConfirmedAtUtc,
        Reviews = item.Reviews.OrderByDescending(x => x.ReviewedAtUtc).Select(review => new ProbationReviewDto
        {
            Id = review.Id,
            Recommendation = review.Recommendation,
            Reason = review.Reason,
            ProposedEndDate = review.ProposedEndDate,
            Status = review.Status,
            ReviewedBy = review.ReviewedBy,
            ReviewedAtUtc = review.ReviewedAtUtc,
            DecidedBy = review.DecidedBy,
            DecidedAtUtc = review.DecidedAtUtc,
            DecisionNotes = review.DecisionNotes
        }).ToList()
    };
}
