using NHRMS.Application.Abstractions;
using NHRMS.Domain;

namespace NHRMS.Application.Features.Employees;

public sealed class OnboardingService(
    IEmployeeRepository employeeRepository,
    IEmployeeOnboardingTaskRepository taskRepository,
    ICurrentActor currentActor,
    IUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<OnboardingTaskDto>> GetTasksAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (await employeeRepository.GetByIdAsync(employeeId, cancellationToken) is null)
            throw new KeyNotFoundException("Employee not found.");

        var tasks = await taskRepository.GetByEmployeeIdAsync(employeeId, cancellationToken);
        return tasks.Select(Map).ToList();
    }

    public async Task<OnboardingTaskDto> CreateTaskAsync(
        Guid employeeId,
        CreateOnboardingTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee is required.", nameof(employeeId));
        var employee = await employeeRepository.GetByIdAsync(employeeId, cancellationToken)
            ?? throw new KeyNotFoundException("Employee not found.");
        if (!employee.IsActive || employee.DateOfLeaving.HasValue)
            throw new InvalidOperationException("Onboarding tasks can only be added to an active employee.");

        var title = request.Title.Trim();
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title is required.", nameof(request.Title));
        if (title.Length > 200)
            throw new ArgumentException("Task title cannot exceed 200 characters.", nameof(request.Title));
        var description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        if (description?.Length > 1000)
            throw new ArgumentException("Task description cannot exceed 1000 characters.", nameof(request.Description));

        var task = new EmployeeOnboardingTask
        {
            EmployeeId = employeeId,
            Title = title,
            Description = description,
            DueDate = request.DueDate?.Date,
            IsRequired = request.IsRequired,
            Status = OnboardingTaskStatus.Pending,
            CreatedBy = currentActor.Name
        };
        await taskRepository.AddAsync(task, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(task);
    }

    public async Task<OnboardingTaskDto> UpdateStatusAsync(
        Guid employeeId,
        Guid taskId,
        OnboardingTaskStatus status,
        CancellationToken cancellationToken = default)
    {
        if (status is not (OnboardingTaskStatus.Completed or OnboardingTaskStatus.Waived))
            throw new ArgumentException("A pending onboarding task can only be completed or waived.", nameof(status));
        var task = await taskRepository.GetByIdAsync(taskId, cancellationToken)
            ?? throw new KeyNotFoundException("Onboarding task not found.");
        if (task.EmployeeId != employeeId)
            throw new KeyNotFoundException("Onboarding task not found for this employee.");
        if (status == OnboardingTaskStatus.Waived && task.IsRequired)
            throw new InvalidOperationException("Required onboarding tasks cannot be waived.");
        if (task.Status != OnboardingTaskStatus.Pending)
            throw new InvalidOperationException("Only pending onboarding tasks can change status.");

        task.Status = status;
        task.CompletedAtUtc = status == OnboardingTaskStatus.Completed ? DateTime.UtcNow : null;
        task.UpdatedAtUtc = DateTime.UtcNow;
        task.UpdatedBy = currentActor.Name;
        await taskRepository.UpdateAsync(task, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(task);
    }

    private static OnboardingTaskDto Map(EmployeeOnboardingTask task) => new()
    {
        Id = task.Id,
        EmployeeId = task.EmployeeId,
        Title = task.Title,
        Description = task.Description,
        DueDate = task.DueDate,
        IsRequired = task.IsRequired,
        Status = task.Status,
        CompletedAtUtc = task.CompletedAtUtc
    };
}
