// <copyright file="DuplicateCandidateDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerManagement.Application.DTOs.Duplicates;

public sealed class DuplicateCandidateDto
{
    public required string EntityType { get; init; }
    public required Guid PrimaryId { get; init; }
    public required Guid CandidateId { get; init; }
    public required string PrimaryDisplayName { get; init; }
    public required string CandidateDisplayName { get; init; }
    public required string Reason { get; init; }
    public decimal Score { get; init; }
}
