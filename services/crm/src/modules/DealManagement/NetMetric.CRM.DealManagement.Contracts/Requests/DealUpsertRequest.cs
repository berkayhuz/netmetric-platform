// <copyright file="DealUpsertRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.DealManagement.Contracts.Requests;

public sealed record DealUpsertRequest(string DealCode, string Name, decimal TotalAmount, DateTime ClosedDate, Guid? OpportunityId, Guid? CompanyId, Guid? OwnerUserId, string? Notes, string? RowVersion);
