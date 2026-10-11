using NHRMS.Application.Abstractions;
using NHRMS.Domain;

namespace NHRMS.Application.Features.Leave;

public sealed class LeaveTypeService
{
    private readonly ILeaveTypeRepository _leaveTypeRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LeaveTypeService(ILeaveTypeRepository leaveTypeRepository, ICompanyRepository companyRepository, IUnitOfWork unitOfWork)
    {
        _leaveTypeRepository = leaveTypeRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<LeaveTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var leaveTypes = await _leaveTypeRepository.GetAllAsync(cancellationToken);
        return leaveTypes.Select(Map).ToList();
    }

    public async Task<LeaveTypeDto> CreateAsync(CreateLeaveTypeRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (request.CompanyId == Guid.Empty)
            throw new ArgumentException("Company is required.", nameof(request.CompanyId));

        var company = await _companyRepository.GetByIdAsync(request.CompanyId, cancellationToken);
        if (company is null)
            throw new InvalidOperationException("Company not found.");

        var name = request.Name.Trim();
        var code = request.Code.Trim();

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Leave type name is required.", nameof(request.Name));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Leave type code is required.", nameof(request.Code));

        if (request.MaxDaysPerYear <= 0)
            throw new ArgumentException("Maximum days per year must be greater than zero.", nameof(request.MaxDaysPerYear));

        if (await _leaveTypeRepository.GetByCodeAsync(request.CompanyId, code, cancellationToken) is not null)
            throw new InvalidOperationException($"Leave type code '{code}' already exists.");

        var leaveType = new LeaveType
        {
            CompanyId = company.Id,
            Name = name,
            Code = code,
            MaxDaysPerYear = request.MaxDaysPerYear,
            IsActive = request.IsActive,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await _leaveTypeRepository.AddAsync(leaveType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(leaveType);
    }

    private static LeaveTypeDto Map(LeaveType leaveType)
        => new()
        {
            Id = leaveType.Id,
            CompanyId = leaveType.CompanyId,
            Name = leaveType.Name,
            Code = leaveType.Code,
            MaxDaysPerYear = leaveType.MaxDaysPerYear,
            IsActive = leaveType.IsActive,
            CreatedAtUtc = leaveType.CreatedAtUtc
        };
}

public sealed class LeaveRequestService
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly ILeaveTypeRepository _leaveTypeRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LeaveRequestService(
        ILeaveRequestRepository leaveRequestRepository,
        ILeaveTypeRepository leaveTypeRepository,
        IEmployeeRepository employeeRepository,
        IUnitOfWork unitOfWork)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _leaveTypeRepository = leaveTypeRepository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<LeaveRequestDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var leaveRequests = await _leaveRequestRepository.GetAllAsync(cancellationToken);
        return leaveRequests.Select(Map).ToList();
    }

    public async Task<LeaveRequestDto> CreateAsync(CreateLeaveRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (request.EmployeeId == Guid.Empty)
            throw new ArgumentException("Employee is required.", nameof(request.EmployeeId));

        if (request.LeaveTypeId == Guid.Empty)
            throw new ArgumentException("Leave type is required.", nameof(request.LeaveTypeId));

        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee is null)
            throw new InvalidOperationException("Employee not found.");

        var leaveType = await _leaveTypeRepository.GetByIdAsync(request.LeaveTypeId, cancellationToken);
        if (leaveType is null)
            throw new InvalidOperationException("Leave type not found.");

        if (request.StartDate > request.EndDate)
            throw new ArgumentException("Start date cannot be later than end date.");

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveTypeId = leaveType.Id,
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate.Date,
            Status = "Pending",
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await _leaveRequestRepository.AddAsync(leaveRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(leaveRequest);
    }

    private static LeaveRequestDto Map(LeaveRequest leaveRequest)
        => new()
        {
            Id = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            LeaveTypeId = leaveRequest.LeaveTypeId,
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            Status = leaveRequest.Status,
            Reason = leaveRequest.Reason,
            CreatedAtUtc = leaveRequest.CreatedAtUtc
        };
}
