IF COL_LENGTH('CatalogProduct', 'CategoryId') IS NULL
BEGIN
    ALTER TABLE [CatalogProduct] ADD [CategoryId] uniqueidentifier NULL;
END;

IF COL_LENGTH('CatalogProduct', 'UnitPrice') IS NULL
BEGIN
    ALTER TABLE [CatalogProduct] ADD [UnitPrice] decimal(18,2) NULL;
END;

IF COL_LENGTH('CatalogProduct', 'CurrencyCode') IS NULL
BEGIN
    ALTER TABLE [CatalogProduct] ADD [CurrencyCode] nvarchar(3) NOT NULL CONSTRAINT [DF_CatalogProduct_CurrencyCode] DEFAULT ('USD');
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_CatalogProduct_CatalogCategory_CategoryId')
BEGIN
    ALTER TABLE [CatalogProduct]
    ADD CONSTRAINT [FK_CatalogProduct_CatalogCategory_CategoryId]
    FOREIGN KEY ([CategoryId]) REFERENCES [CatalogCategory]([Id]) ON DELETE SET NULL;
END;

IF EXISTS (
    SELECT 1
    FROM sys.default_constraints
    WHERE name = 'DF_CatalogProduct_CurrencyCode')
BEGIN
    ALTER TABLE [CatalogProduct] DROP CONSTRAINT [DF_CatalogProduct_CurrencyCode];
END;
