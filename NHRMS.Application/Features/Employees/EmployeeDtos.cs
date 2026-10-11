namespace NHRMS.Application.Features.Employees;

public sealed class CreateEmployeeRequest
{
    public Guid CompanyId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime DateOfJoining { get; set; }
    public DateTime? DateOfLeaving { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? DesignationId { get; set; }
    public Guid? EmploymentTypeId { get; set; }
}

public sealed class EmployeeDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime DateOfJoining { get; set; }
    public DateTime? DateOfLeaving { get; set; }
    public bool IsActive { get; set; }
}
