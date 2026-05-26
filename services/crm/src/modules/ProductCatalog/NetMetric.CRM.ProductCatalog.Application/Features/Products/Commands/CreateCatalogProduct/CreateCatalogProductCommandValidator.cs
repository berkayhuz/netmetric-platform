// <copyright file="CreateCatalogProductCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;

namespace NetMetric.CRM.ProductCatalog.Application.Features.Products.Commands.CreateCatalogProduct;

public sealed class CreateCatalogProductCommandValidator : AbstractValidator<CreateCatalogProductCommand>
{
    public CreateCatalogProductCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).When(x => x.UnitPrice.HasValue);
        RuleFor(x => x.CurrencyCode).NotEmpty().Length(3);
    }
}
