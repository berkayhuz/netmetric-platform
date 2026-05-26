// <copyright file="SupportInboxConnectionDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.SupportInboxIntegration.Contracts.DTOs;

public sealed record SupportInboxConnectionDto(Guid Id, string Name, string Provider, string EmailAddress, string Host, int Port, string Username, bool UseSsl, bool IsActive);
