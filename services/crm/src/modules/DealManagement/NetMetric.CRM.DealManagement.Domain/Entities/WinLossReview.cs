// <copyright file="WinLossReview.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.DealManagement.Domain.Entities;

public class WinLossReview : EntityBase
{
    public Guid DealId { get; set; }
    public string Outcome { get; set; } = null!;
    public string? Summary { get; set; }
    public string? Strengths { get; set; }
    public string? Risks { get; set; }
    public string? CompetitorName { get; set; }
    public decimal? CompetitorPrice { get; set; }
    public string? CustomerFeedback { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedByUserId { get; set; }
}
