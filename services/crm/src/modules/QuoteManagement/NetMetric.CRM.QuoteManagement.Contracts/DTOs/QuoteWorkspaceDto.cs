// <copyright file="QuoteWorkspaceDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.QuoteManagement.Contracts.DTOs;

public sealed record QuoteWorkspaceDto(QuoteDetailDto Quote, bool CanEdit, bool CanSubmit, bool CanApprove, bool CanReject, bool CanSend, bool CanAccept, bool CanDecline, bool CanExpire);
