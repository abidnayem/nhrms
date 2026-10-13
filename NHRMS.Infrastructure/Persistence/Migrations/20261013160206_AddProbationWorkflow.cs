using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NHRMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProbationWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProbationCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlannedEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtendedEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ConfirmedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProbationCases", x => x.Id);
                    table.CheckConstraint("CK_ProbationCases_DateRange", "[PlannedEndDate] > [StartDate]");
                    table.ForeignKey(
                        name: "FK_ProbationCases_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProbationReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProbationCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Recommendation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ProposedEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReviewedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecidedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DecidedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DecisionNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProbationReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProbationReviews_ProbationCases_ProbationCaseId",
                        column: x => x.ProbationCaseId,
                        principalTable: "ProbationCases",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProbationCases_EmployeeId",
                table: "ProbationCases",
                column: "EmployeeId",
                unique: true,
                filter: "[Status] <> 'Confirmed'");

            migrationBuilder.CreateIndex(
                name: "IX_ProbationReviews_ProbationCaseId",
                table: "ProbationReviews",
                column: "ProbationCaseId",
                unique: true,
                filter: "[Status] = 'Pending'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProbationReviews");

            migrationBuilder.DropTable(
                name: "ProbationCases");
        }
    }
}
