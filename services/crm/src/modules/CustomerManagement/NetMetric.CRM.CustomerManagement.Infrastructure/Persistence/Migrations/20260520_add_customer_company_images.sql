IF COL_LENGTH('Customers', 'ProfileImageStorageKey') IS NULL
BEGIN
    ALTER TABLE [Customers] ADD [ProfileImageStorageKey] nvarchar(512) NULL;
END;

IF COL_LENGTH('Customers', 'ProfileImageUrl') IS NULL
BEGIN
    ALTER TABLE [Customers] ADD [ProfileImageUrl] nvarchar(2048) NULL;
END;

IF COL_LENGTH('Customers', 'ProfileImageContentType') IS NULL
BEGIN
    ALTER TABLE [Customers] ADD [ProfileImageContentType] nvarchar(128) NULL;
END;

IF COL_LENGTH('Companies', 'LogoStorageKey') IS NULL
BEGIN
    ALTER TABLE [Companies] ADD [LogoStorageKey] nvarchar(512) NULL;
END;

IF COL_LENGTH('Companies', 'LogoUrl') IS NULL
BEGIN
    ALTER TABLE [Companies] ADD [LogoUrl] nvarchar(2048) NULL;
END;

IF COL_LENGTH('Companies', 'LogoContentType') IS NULL
BEGIN
    ALTER TABLE [Companies] ADD [LogoContentType] nvarchar(128) NULL;
END;
