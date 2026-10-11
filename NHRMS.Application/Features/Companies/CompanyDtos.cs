namespace NHRMS.Application.Features.Companies;

public sealed class CreateCompanyRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? TimeZoneId { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class CompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? TimeZoneId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
