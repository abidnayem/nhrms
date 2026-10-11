namespace NHRMS.Domain;

public class Role : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Permission> Permissions { get; private set; } = new HashSet<Permission>();
}

public class Permission : AuditableEntity
{
    public Guid RoleId { get; set; }
    public Role? Role { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UserAccount : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<UserRole> UserRoles { get; private set; } = new HashSet<UserRole>();
}

public class UserRole : AuditableEntity
{
    public Guid UserId { get; set; }
    public UserAccount? User { get; set; }

    public Guid RoleId { get; set; }
    public Role? Role { get; set; }

    public bool IsActive { get; set; } = true;
}

public class Attendance : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime AttendanceDate { get; set; }
    public DateTime CheckInUtc { get; set; }
    public DateTime? CheckOutUtc { get; set; }
    public decimal HoursWorked { get; set; }
    public string Status { get; set; } = "Present";
    public string? Notes { get; set; }
}

public class LeaveType : AuditableEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int MaxDaysPerYear { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<LeaveRequest> LeaveRequests { get; private set; } = new HashSet<LeaveRequest>();
}

public class LeaveRequest : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public Guid LeaveTypeId { get; set; }
    public LeaveType? LeaveType { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Reason { get; set; }
    public string? ApprovedBy { get; set; }
}

public class PayrollCycle : AuditableEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime RunDateUtc { get; set; } = DateTime.UtcNow;
    public decimal TotalGross { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNet { get; set; }
    public string Status { get; set; } = "Draft";

    public ICollection<Payslip> Payslips { get; private set; } = new HashSet<Payslip>();
}

public class Payslip : AuditableEntity
{
    public Guid PayrollCycleId { get; set; }
    public PayrollCycle? PayrollCycle { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public decimal GrossSalary { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetSalary { get; set; }
    public string Status { get; set; } = "Draft";
}
