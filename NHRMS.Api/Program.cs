using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
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
using NHRMS.Api.Middleware;
using NHRMS.Api.Security;
using NHRMS.Application.Abstractions;
using NHRMS.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var jwtKey = builder.Configuration["Authentication:JwtKey"];
if (string.IsNullOrWhiteSpace(jwtKey) && builder.Environment.IsDevelopment())
{
    jwtKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    builder.Configuration["Authentication:JwtKey"] = jwtKey;
}
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Configure Authentication:JwtKey with at least 32 UTF-8 bytes.");
var bootstrapKey = builder.Configuration["Authentication:BootstrapKey"];
if (!string.IsNullOrWhiteSpace(bootstrapKey) && Encoding.UTF8.GetByteCount(bootstrapKey) < 32)
    throw new InvalidOperationException("Authentication:BootstrapKey must contain at least 32 UTF-8 bytes.");

builder.Services.AddCors(options =>
{
    options.AddPolicy("NhrmsWeb", policy =>
        policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentActor, CurrentActor>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = "NHRMS",
        ValidateAudience = true,
        ValidAudience = "NHRMS.Web",
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Name
    };
});
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.AddPolicy("EmployeeRead", policy => policy.RequireClaim("permission", "EMPLOYEE_READ"));
    options.AddPolicy("EmployeeWrite", policy => policy.RequireClaim("permission", "EMPLOYEE_WRITE"));
    options.AddPolicy("OnboardingManage", policy => policy.RequireClaim("permission", "ONBOARDING_MANAGE"));
    options.AddPolicy("ProbationManage", policy => policy.RequireClaim("permission", "PROBATION_MANAGE"));
    options.AddPolicy("ProbationReview", policy => policy.RequireClaim("permission", "PROBATION_REVIEW"));
    options.AddPolicy("ProbationApprove", policy => policy.RequireClaim("permission", "PROBATION_APPROVE"));
    options.AddPolicy("EmploymentChangeManage", policy => policy.RequireClaim("permission", "EMPLOYMENT_CHANGE_MANAGE"));
    options.AddPolicy("EmploymentChangeApprove", policy => policy.RequireClaim("permission", "EMPLOYMENT_CHANGE_APPROVE"));
    options.AddPolicy("DisciplineRead", policy => policy.RequireClaim("permission", "DISCIPLINE_READ"));
    options.AddPolicy("DisciplineManage", policy => policy.RequireClaim("permission", "DISCIPLINE_MANAGE"));
    options.AddPolicy("DisciplineDecide", policy => policy.RequireClaim("permission", "DISCIPLINE_DECIDE"));
    options.AddPolicy("UserAdmin", policy => policy.RequireClaim("permission", "USER_ADMIN"));
    options.AddPolicy("OrganizationAdmin", policy => policy.RequireClaim("permission", "ORGANIZATION_ADMIN"));
    options.AddPolicy("OrganizationRead", policy => policy.RequireClaim("permission", "ORGANIZATION_READ"));
    options.AddPolicy("LeaveManage", policy => policy.RequireClaim("permission", "LEAVE_MANAGE"));
    options.AddPolicy("AttendanceManage", policy => policy.RequireClaim("permission", "ATTENDANCE_MANAGE"));
    options.AddPolicy("PayrollRun", policy => policy.RequireClaim("permission", "PAYROLL_RUN"));
    options.AddPolicy("ReportRead", policy => policy.RequireClaim("permission", "REPORT_READ"));
});
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("AuthEndpoints", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
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
builder.Services.AddScoped<OnboardingService>();
builder.Services.AddScoped<ProbationService>();
builder.Services.AddScoped<EmploymentChangeService>();
builder.Services.AddScoped<DisciplinaryService>();
builder.Services.AddScoped<UserAccountService>();
builder.Services.AddScoped<AdminBootstrapService>();

var app = builder.Build();

if (args.Contains("--migrate-and-seed", StringComparer.OrdinalIgnoreCase))
{
    await MigrateAndSeedAsync(app);
    return;
}

if (app.Environment.IsDevelopment() && !args.Contains("--skip-startup-migrate", StringComparer.OrdinalIgnoreCase))
    await MigrateAndSeedAsync(app);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();

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
        "text/html")).AllowAnonymous();
}

var httpsConfigured = app.Urls.Any(url => url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) ||
    !string.IsNullOrWhiteSpace(app.Configuration["ASPNETCORE_HTTPS_PORT"]) ||
    !string.IsNullOrWhiteSpace(app.Configuration["HTTPS_PORT"]);
if (httpsConfigured)
    app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("NhrmsWeb");
app.UseRateLimiter();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "ok", timestampUtc = DateTime.UtcNow })).AllowAnonymous();
app.MapControllers();

app.Run();

static async Task MigrateAndSeedAsync(WebApplication app)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<NHRMS.Infrastructure.Persistence.NhrmsDbContext>();
    await db.Database.MigrateAsync();
    await NHRMS.Infrastructure.Persistence.NhrmsDbSeeder.SeedAsync(db);
}
