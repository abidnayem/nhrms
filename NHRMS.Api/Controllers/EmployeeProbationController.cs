using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHRMS.Application.Features.Employees;

namespace NHRMS.Api.Controllers;

[ApiController]
[Route("api/employees/{employeeId:guid}/probation")]
public sealed class EmployeeProbationController(ProbationService probationService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "ProbationReview")]
    public async Task<ActionResult<IReadOnlyList<ProbationCaseDto>>> GetCases(
        Guid employeeId,
        CancellationToken cancellationToken)
        => Ok(await probationService.GetCasesAsync(employeeId, cancellationToken));

    [HttpPost]
    [Authorize(Policy = "ProbationManage")]
    public async Task<ActionResult<ProbationCaseDto>> Start(
        Guid employeeId,
        StartProbationRequest request,
        CancellationToken cancellationToken)
    {
        var probationCase = await probationService.StartAsync(employeeId, request, cancellationToken);
        return CreatedAtAction(nameof(GetCases), new { employeeId }, probationCase);
    }
}

[ApiController]
[Route("api/probation/{caseId:guid}/reviews")]
public sealed class ProbationReviewsController(ProbationService probationService) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "ProbationReview")]
    public async Task<ActionResult<ProbationCaseDto>> Recommend(
        Guid caseId,
        CreateProbationReviewRequest request,
        CancellationToken cancellationToken)
        => Ok(await probationService.RecommendAsync(caseId, request, cancellationToken));

    [HttpPut("{reviewId:guid}/decision")]
    [Authorize(Policy = "ProbationApprove")]
    public async Task<ActionResult<ProbationCaseDto>> Decide(
        Guid caseId,
        Guid reviewId,
        DecideProbationReviewRequest request,
        CancellationToken cancellationToken)
        => Ok(await probationService.DecideAsync(caseId, reviewId, request, cancellationToken));
}
