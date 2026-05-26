// <copyright file="BulkAssignDealsOwnerRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.DealManagement.Contracts.Requests;

public sealed record BulkAssignDealsOwnerRequest(IReadOnlyList<Guid> DealIds, Guid? OwnerUserId);
