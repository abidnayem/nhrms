namespace NHRMS.Domain;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
}

public abstract class AuditableEntity : BaseEntity
{
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

public class Company : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? TimeZoneId { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Branch> Branches { get; private set; } = new HashSet<Branch>();
    public ICollection<Department> Departments { get; private set; } = new HashSet<Department>();
    public ICollection<Designation> Designations { get; private set; } = new HashSet<Designation>();
    public ICollection<Employee> Employees { get; private set; } = new HashSet<Employee>();
}

public class Branch : AuditableEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Department> Departments { get; private set; } = new HashSet<Department>();
}

public class Department : AuditableEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }

    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public Guid? ParentDepartmentId { get; set; }
    public Department? ParentDepartment { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Department> ChildDepartments { get; private set; } = new HashSet<Department>();
    public ICollection<Employee> Employees { get; private set; } = new HashSet<Employee>();
}

public class Designation : AuditableEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Employee> Employees { get; private set; } = new HashSet<Employee>();
}

public class EmploymentType : AuditableEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Employee> Employees { get; private set; } = new HashSet<Employee>();
}

public class Employee : AuditableEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public Guid? DesignationId { get; set; }
    public Designation? Designation { get; set; }

    public Guid? EmploymentTypeId { get; set; }
    public EmploymentType? EmploymentType { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime DateOfJoining { get; set; }
    public DateTime? DateOfLeaving { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<EmployeeOnboardingTask> OnboardingTasks { get; private set; } = new HashSet<EmployeeOnboardingTask>();
}
