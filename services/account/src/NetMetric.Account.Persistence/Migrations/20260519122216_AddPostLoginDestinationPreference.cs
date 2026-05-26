using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetMetric.Account.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPostLoginDestinationPreference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PostLoginDestination",
                table: "account_user_preferences",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Account");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostLoginDestination",
                table: "account_user_preferences");
        }
    }
}
