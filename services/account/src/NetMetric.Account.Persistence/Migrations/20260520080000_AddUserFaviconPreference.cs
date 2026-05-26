// <copyright file="20260520080000_AddUserFaviconPreference.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace NetMetric.Account.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AccountDbContext))]
    [Migration("20260520080000_AddUserFaviconPreference")]
    public partial class AddUserFaviconPreference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FaviconMediaAssetId",
                table: "account_user_preferences",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaviconUrl",
                table: "account_user_preferences",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FaviconMediaAssetId",
                table: "account_user_preferences");

            migrationBuilder.DropColumn(
                name: "FaviconUrl",
                table: "account_user_preferences");
        }
    }
}
