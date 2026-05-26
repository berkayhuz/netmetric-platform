// <copyright file="QuoteItemDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.QuoteManagement.Contracts.DTOs;

public sealed record QuoteItemDto(Guid Id, Guid ProductId, string? Description, int Quantity, decimal UnitPrice, decimal DiscountRate, decimal TaxRate, decimal LineTotal);
