using Microsoft.AspNetCore.Mvc;
using NHRMS.Application.Features.Companies;

namespace NHRMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CompaniesController : ControllerBase
{
    private readonly CompanyService _companyService;

    public CompaniesController(CompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompanyDto>>> GetAll(CancellationToken cancellationToken)
    {
        var companies = await _companyService.GetAllAsync(cancellationToken);
        return Ok(companies);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyDto>> Create([FromBody] CreateCompanyRequest request, CancellationToken cancellationToken)
    {
        var company = await _companyService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { }, company);
    }
}
