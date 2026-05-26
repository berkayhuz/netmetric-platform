using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetMetric.Account.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCrmDashboardPreferencesJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CrmDashboardPreferencesJson",
                table: "account_user_preferences",
                type: "nvarchar(max)",
                maxLength: 200000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CrmDashboardPreferencesJson",
                table: "account_user_preferences");
        }
    }
}
