// <copyright file="ProposalTemplateDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.QuoteManagement.Contracts.DTOs;

public sealed record ProposalTemplateDto(Guid Id, string Name, string? SubjectTemplate, string BodyTemplate, bool IsDefault, bool IsActive, string? Notes);
