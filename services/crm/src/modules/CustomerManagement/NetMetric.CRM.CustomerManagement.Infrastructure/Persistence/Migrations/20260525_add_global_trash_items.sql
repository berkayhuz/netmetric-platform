IF OBJECT_ID(N'[dbo].[GlobalTrashItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[GlobalTrashItems]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [EntityType] NVARCHAR(64) NOT NULL,
        [EntityId] UNIQUEIDENTIFIER NOT NULL,
        [DisplayName] NVARCHAR(256) NOT NULL,
        [Summary] NVARCHAR(512) NULL,
        [SourceModule] NVARCHAR(64) NOT NULL,
        [OriginalRoute] NVARCHAR(256) NULL,
        [DeletedAtUtc] DATETIME2 NOT NULL,
        [DeletedByUserId] UNIQUEIDENTIFIER NULL,
        [DeletedByDisplayName] NVARCHAR(256) NULL,
        [ExpiresAtUtc] DATETIME2 NOT NULL,
        [Status] NVARCHAR(32) NOT NULL,
        [MetadataJson] NVARCHAR(4000) NULL,
        [AuditCorrelationId] NVARCHAR(128) NULL,
        [RestoredAtUtc] DATETIME2 NULL,
        [RestoredByUserId] UNIQUEIDENTIFIER NULL,
        [PurgedAtUtc] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [CreatedBy] NVARCHAR(MAX) NULL,
        [UpdatedBy] NVARCHAR(MAX) NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT [DF_GlobalTrashItems_IsDeleted] DEFAULT (0),
        [DeletedAt] DATETIME2 NULL,
        [DeletedBy] NVARCHAR(MAX) NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_GlobalTrashItems_IsActive] DEFAULT (1),
        [RowVersion] ROWVERSION NOT NULL,
        CONSTRAINT [PK_GlobalTrashItems] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_GlobalTrashItems_TenantId_Status_ExpiresAtUtc' AND object_id = OBJECT_ID(N'[dbo].[GlobalTrashItems]'))
BEGIN
    CREATE INDEX [IX_GlobalTrashItems_TenantId_Status_ExpiresAtUtc]
        ON [dbo].[GlobalTrashItems]([TenantId], [Status], [ExpiresAtUtc]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_GlobalTrashItems_TenantId_EntityType_DeletedAtUtc' AND object_id = OBJECT_ID(N'[dbo].[GlobalTrashItems]'))
BEGIN
    CREATE INDEX [IX_GlobalTrashItems_TenantId_EntityType_DeletedAtUtc]
        ON [dbo].[GlobalTrashItems]([TenantId], [EntityType], [DeletedAtUtc]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_GlobalTrashItems_TenantId_EntityType_EntityId' AND object_id = OBJECT_ID(N'[dbo].[GlobalTrashItems]'))
BEGIN
    CREATE INDEX [IX_GlobalTrashItems_TenantId_EntityType_EntityId]
        ON [dbo].[GlobalTrashItems]([TenantId], [EntityType], [EntityId]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_GlobalTrashItems_ActivePerEntity' AND object_id = OBJECT_ID(N'[dbo].[GlobalTrashItems]'))
BEGIN
    CREATE UNIQUE INDEX [IX_GlobalTrashItems_ActivePerEntity]
        ON [dbo].[GlobalTrashItems]([TenantId], [EntityType], [EntityId], [Status])
        WHERE [Status] = N'active';
END;
GO
