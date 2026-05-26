// <copyright file="OpportunityProductUpsertRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.OpportunityManagement.Contracts.Requests;

public sealed class OpportunityProductUpsertRequest { public Guid ProductId { get; set; } public int Quantity { get; set; } public decimal UnitPrice { get; set; } public decimal DiscountRate { get; set; } public decimal VatRate { get; set; } }
