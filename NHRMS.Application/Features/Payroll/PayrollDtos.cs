namespace NHRMS.Application.Features.Payroll;

public sealed class CreatePayrollCycleRequest
{
    public Guid CompanyId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalDeductions { get; set; }
}

public sealed class PayrollCycleDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNet { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RunDateUtc { get; set; }
}
