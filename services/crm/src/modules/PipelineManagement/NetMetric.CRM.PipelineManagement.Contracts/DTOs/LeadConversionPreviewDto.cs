// <copyright file="LeadConversionPreviewDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.PipelineManagement.Contracts.DTOs;

public sealed record LeadConversionPreviewDto(Guid LeadId, string LeadCode, string FullName, string? CompanyName, string? Email, decimal? EstimatedBudget, bool AlreadyConverted, Guid? ConvertedCustomerId);
