// <copyright file="ImportFailureDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerManagement.Application.DTOs.Imports;

public sealed record ImportFailureDto(
    int RowNumber,
    string Message);
