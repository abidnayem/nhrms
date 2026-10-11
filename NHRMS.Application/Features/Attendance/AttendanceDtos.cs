namespace NHRMS.Application.Features.Attendance;

public sealed class CreateAttendanceRequest
{
    public Guid EmployeeId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public DateTime CheckInUtc { get; set; }
    public DateTime? CheckOutUtc { get; set; }
    public string? Notes { get; set; }
}

public sealed class AttendanceDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public DateTime CheckInUtc { get; set; }
    public DateTime? CheckOutUtc { get; set; }
    public decimal HoursWorked { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
