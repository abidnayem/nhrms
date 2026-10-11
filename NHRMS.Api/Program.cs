using NHRMS.Application.Features.Attendance;
using NHRMS.Application.Features.Branches;
using NHRMS.Application.Features.Companies;
using NHRMS.Application.Features.Departments;
using NHRMS.Application.Features.Designations;
using NHRMS.Application.Features.Employees;
using NHRMS.Application.Features.EmploymentTypes;
using NHRMS.Application.Features.Leave;
using NHRMS.Application.Features.Payroll;
using NHRMS.Application.Features.Roles;
using NHRMS.Application.Features.Users;
using NHRMS.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddNhrmsInfrastructure(builder.Configuration);
builder.Services.AddScoped<CompanyService>();
builder.Services.AddScoped<BranchService>();
builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<DesignationService>();
builder.Services.AddScoped<EmploymentTypeService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<LeaveTypeService>();
builder.Services.AddScoped<LeaveRequestService>();
builder.Services.AddScoped<PayrollCycleService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<UserAccountService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapGet("/swagger", () => Results.Content(
        """
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            <title>NHRMS API Swagger</title>
            <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/swagger-ui-dist@5/swagger-ui.css" />
            <style>
                html, body { margin: 0; height: 100%; background: #f5f7fb; }
                body { font-family: Arial, sans-serif; }
                #swagger-ui { height: 100vh; }
            </style>
        </head>
        <body>
            <div id="swagger-ui"></div>
            <script src="https://cdn.jsdelivr.net/npm/swagger-ui-dist@5/swagger-ui-bundle.js"></script>
            <script>
                window.onload = function () {
                    SwaggerUIBundle({
                        url: '/openapi/v1.json',
                        dom_id: '#swagger-ui',
                        deepLinking: true,
                        presets: [SwaggerUIBundle.presets.apis, SwaggerUIBundle.SwaggerUIStandalonePreset],
                        layout: 'BaseLayout'
                    });
                };
            </script>
        </body>
        </html>
        """,
        "text/html"));
}

app.UseHttpsRedirection();
app.MapGet("/health", () => Results.Ok(new { status = "ok", timestampUtc = DateTime.UtcNow }));
app.MapControllers();

app.Run();
