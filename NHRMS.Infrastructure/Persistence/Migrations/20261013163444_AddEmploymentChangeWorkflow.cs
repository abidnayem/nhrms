using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NHRMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmploymentChangeWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmploymentChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangeType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PreviousDepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NewDepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PreviousDesignationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NewDesignationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PreviousEmploymentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NewEmploymentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RequestedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequestedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecidedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DecidedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DecisionNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AppliedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmploymentChanges_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentChanges_EmployeeId",
                table: "EmploymentChanges",
                column: "EmployeeId",
                unique: true,
                filter: "[Status] IN ('Pending', 'Approved')");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentChanges_Status_EffectiveDate",
                table: "EmploymentChanges",
                columns: new[] { "Status", "EffectiveDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmploymentChanges");
        }
    }
}
