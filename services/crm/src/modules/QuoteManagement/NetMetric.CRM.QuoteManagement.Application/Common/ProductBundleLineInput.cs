// <copyright file="ProductBundleLineInput.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.QuoteManagement.Application.Common;

public sealed record ProductBundleLineInput(Guid ProductId, int Quantity, bool IsOptional);
