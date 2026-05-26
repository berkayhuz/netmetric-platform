// <copyright file="LostReasonDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.PipelineManagement.Contracts.DTOs;

public sealed record LostReasonDto(Guid Id, string Name, string? Description, bool IsDefault, string? RowVersion);
