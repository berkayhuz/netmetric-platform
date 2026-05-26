IF COL_LENGTH('CatalogProduct', 'DefaultDiscountRate') IS NULL
BEGIN
    ALTER TABLE CatalogProduct
    ADD DefaultDiscountRate decimal(5,2) NOT NULL CONSTRAINT DF_CatalogProduct_DefaultDiscountRate DEFAULT (0);
END;

IF COL_LENGTH('CatalogProduct', 'DefaultTaxRate') IS NULL
BEGIN
    ALTER TABLE CatalogProduct
    ADD DefaultTaxRate decimal(5,2) NOT NULL CONSTRAINT DF_CatalogProduct_DefaultTaxRate DEFAULT (0);
END;
