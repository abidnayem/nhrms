using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHRMS.Application.Features.Employees;
using NHRMS.Domain;

namespace NHRMS.Api.Controllers;

[ApiController]
[Authorize(Policy = "OnboardingManage")]
[Route("api/employees/{employeeId:guid}/onboarding")]
public sealed class EmployeeOnboardingController(OnboardingService onboardingService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OnboardingTaskDto>>> GetTasks(
        Guid employeeId,
        CancellationToken cancellationToken)
        => Ok(await onboardingService.GetTasksAsync(employeeId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<OnboardingTaskDto>> CreateTask(
        Guid employeeId,
        CreateOnboardingTaskRequest request,
        CancellationToken cancellationToken)
    {
        var task = await onboardingService.CreateTaskAsync(employeeId, request, cancellationToken);
        return CreatedAtAction(nameof(GetTasks), new { employeeId }, task);
    }

    [HttpPut("{taskId:guid}/status")]
    public async Task<ActionResult<OnboardingTaskDto>> UpdateStatus(
        Guid employeeId,
        Guid taskId,
        UpdateOnboardingTaskStatusRequest request,
        CancellationToken cancellationToken)
        => Ok(await onboardingService.UpdateStatusAsync(employeeId, taskId, request.Status, cancellationToken));
}
