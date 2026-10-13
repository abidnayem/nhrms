using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NHRMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueBootstrapAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBootstrapAccount",
                table: "UserAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_IsBootstrapAccount",
                table: "UserAccounts",
                column: "IsBootstrapAccount",
                unique: true,
                filter: "[IsBootstrapAccount] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAccounts_IsBootstrapAccount",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "IsBootstrapAccount",
                table: "UserAccounts");
        }
    }
}
