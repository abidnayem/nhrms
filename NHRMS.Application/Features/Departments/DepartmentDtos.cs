namespace NHRMS.Application.Features.Departments;

public sealed class CreateDepartmentRequest
{
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class DepartmentDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
