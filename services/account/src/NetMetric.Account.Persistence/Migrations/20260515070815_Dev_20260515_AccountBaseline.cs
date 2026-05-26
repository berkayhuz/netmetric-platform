// <copyright file="20260515070815_Dev_20260515_AccountBaseline.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetMetric.Account.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Dev_20260515_AccountBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account_audit_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    MetadataJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_audit_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "account_media_assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Module = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    SafeFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Sha256Hash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    StorageProvider = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    PublicUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    Visibility = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_media_assets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "account_notification_preferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_notification_preferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "account_outbox_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeadLetteredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NextAttemptAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_outbox_messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "account_trusted_devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    DeviceTokenHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    TrustedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RevocationReason = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_trusted_devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "account_user_consents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsentType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DecidedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    VersionToken = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_user_consents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "account_user_notification_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_user_notification_states", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "account_user_preferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Theme = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateFormat = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DefaultOrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_user_preferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "account_user_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(220)", maxLength: 220, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    AvatarMediaAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Culture = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_user_profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "account_user_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RefreshTokenHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    ApproximateLocation = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RevocationReason = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_user_sessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_audit_entries_TenantId_UserId_OccurredAt",
                table: "account_audit_entries",
                columns: new[] { "TenantId", "UserId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_account_media_assets_TenantId_Module_Purpose_Status",
                table: "account_media_assets",
                columns: new[] { "TenantId", "Module", "Purpose", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_account_media_assets_TenantId_OwnerUserId",
                table: "account_media_assets",
                columns: new[] { "TenantId", "OwnerUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_account_notification_preferences_TenantId_UserId_Channel_Category",
                table: "account_notification_preferences",
                columns: new[] { "TenantId", "UserId", "Channel", "Category" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_account_outbox_messages_DeadLetteredAt_OccurredAt",
                table: "account_outbox_messages",
                columns: new[] { "DeadLetteredAt", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_account_outbox_messages_ProcessedAt_NextAttemptAt_OccurredAt",
                table: "account_outbox_messages",
                columns: new[] { "ProcessedAt", "NextAttemptAt", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_account_outbox_messages_TenantId_Type_OccurredAt",
                table: "account_outbox_messages",
                columns: new[] { "TenantId", "Type", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_account_trusted_devices_TenantId_UserId_DeviceTokenHash",
                table: "account_trusted_devices",
                columns: new[] { "TenantId", "UserId", "DeviceTokenHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_account_trusted_devices_TenantId_UserId_ExpiresAt",
                table: "account_trusted_devices",
                columns: new[] { "TenantId", "UserId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_account_user_consents_TenantId_UserId_ConsentType_DecidedAt",
                table: "account_user_consents",
                columns: new[] { "TenantId", "UserId", "ConsentType", "DecidedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_account_user_notification_states_TenantId_UserId_IsDeleted",
                table: "account_user_notification_states",
                columns: new[] { "TenantId", "UserId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_account_user_notification_states_TenantId_UserId_NotificationId",
                table: "account_user_notification_states",
                columns: new[] { "TenantId", "UserId", "NotificationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_account_user_preferences_TenantId_UserId",
                table: "account_user_preferences",
                columns: new[] { "TenantId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_account_user_profiles_TenantId_UserId",
                table: "account_user_profiles",
                columns: new[] { "TenantId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_account_user_sessions_TenantId_UserId_LastSeenAt",
                table: "account_user_sessions",
                columns: new[] { "TenantId", "UserId", "LastSeenAt" });

            migrationBuilder.CreateIndex(
                name: "IX_account_user_sessions_TenantId_UserId_RevokedAt_ExpiresAt",
                table: "account_user_sessions",
                columns: new[] { "TenantId", "UserId", "RevokedAt", "ExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_audit_entries");

            migrationBuilder.DropTable(
                name: "account_media_assets");

            migrationBuilder.DropTable(
                name: "account_notification_preferences");

            migrationBuilder.DropTable(
                name: "account_outbox_messages");

            migrationBuilder.DropTable(
                name: "account_trusted_devices");

            migrationBuilder.DropTable(
                name: "account_user_consents");

            migrationBuilder.DropTable(
                name: "account_user_notification_states");

            migrationBuilder.DropTable(
                name: "account_user_preferences");

            migrationBuilder.DropTable(
                name: "account_user_profiles");

            migrationBuilder.DropTable(
                name: "account_user_sessions");
        }
    }
}
