using NHRMS.Application.Abstractions;
using NHRMS.Domain;
using DomainAttendance = NHRMS.Domain.Attendance;

namespace NHRMS.Application.Features.Attendance;

public sealed class AttendanceService
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AttendanceService(IAttendanceRepository attendanceRepository, IUnitOfWork unitOfWork)
    {
        _attendanceRepository = attendanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<AttendanceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var attendance = await _attendanceRepository.GetAllAsync(cancellationToken);
        return attendance.Select(Map).ToList();
    }

    public async Task<AttendanceDto> CreateAsync(CreateAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.EmployeeId == Guid.Empty)
        {
            throw new ArgumentException("Employee is required.", nameof(request.EmployeeId));
        }

        if (request.CheckOutUtc.HasValue && request.CheckOutUtc.Value <= request.CheckInUtc)
        {
            throw new ArgumentException("Check out time must be later than check in time.");
        }

        var hoursWorked = request.CheckOutUtc.HasValue
            ? Convert.ToDecimal((request.CheckOutUtc.Value - request.CheckInUtc).TotalHours)
            : 0m;

        var attendance = new DomainAttendance
        {
            EmployeeId = request.EmployeeId,
            AttendanceDate = request.AttendanceDate.Date,
            CheckInUtc = request.CheckInUtc,
            CheckOutUtc = request.CheckOutUtc,
            HoursWorked = hoursWorked,
            Status = request.CheckOutUtc.HasValue ? "Present" : "Checked In",
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await _attendanceRepository.AddAsync(attendance, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(attendance);
    }

    private static AttendanceDto Map(DomainAttendance attendance)
        => new()
        {
            Id = attendance.Id,
            EmployeeId = attendance.EmployeeId,
            AttendanceDate = attendance.AttendanceDate,
            CheckInUtc = attendance.CheckInUtc,
            CheckOutUtc = attendance.CheckOutUtc,
            HoursWorked = attendance.HoursWorked,
            Status = attendance.Status,
            Notes = attendance.Notes,
            CreatedAtUtc = attendance.CreatedAtUtc
        };
}
