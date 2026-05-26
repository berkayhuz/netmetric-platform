// <copyright file="ForecastCategoryMapping.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;
using NetMetric.Entities;

namespace NetMetric.CRM.PipelineManagement.Domain.Entities;

public class ForecastCategoryMapping : AuditableEntity
{
    public string ExternalStageName { get; set; } = string.Empty;
    public ForecastCategory ForecastCategory { get; set; }
}
