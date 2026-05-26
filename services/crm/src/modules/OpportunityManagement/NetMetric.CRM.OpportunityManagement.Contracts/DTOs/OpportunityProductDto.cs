// <copyright file="OpportunityProductDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.OpportunityManagement.Contracts.DTOs;

public sealed record OpportunityProductDto(Guid Id, Guid ProductId, int Quantity, decimal UnitPrice, decimal DiscountRate, decimal VatRate);
