// <copyright file="Lead.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Sales;

public class Lead : AuditableEntity
{
    public string LeadCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }
    public string? JobTitle { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? MobilePhone { get; set; }
    public LeadSourceType Source { get; set; } = LeadSourceType.Manual;
    public LeadStatusType Status { get; set; } = LeadStatusType.New;
    public PriorityType Priority { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public decimal? EstimatedBudget { get; set; }
    public DateTime? NextContactDate { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? ConvertedCustomerId { get; set; }
    public Customer? ConvertedCustomer { get; set; }

    // Attribution
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public string? UtmTerm { get; set; }
    public string? UtmContent { get; set; }
    public string? ReferrerUrl { get; set; }

    // Scoring & AI
    public decimal FitScore { get; set; }
    public decimal EngagementScore { get; set; }
    public decimal TotalScore { get; set; }
    public LeadGradeType Grade { get; set; }
    public bool IsAiScored { get; set; }
    public decimal? AiScore { get; set; }

    // SLA
    public DateTime? SlaTargetTime { get; set; }
    public DateTime? FirstContactTime { get; set; }
    public bool SlaBreached { get; set; }
    public int? TimeToContactMinutes { get; set; }

    // Lifecycle & Qualification
    public QualificationFrameworkType QualificationFramework { get; set; }
    public string? QualificationData { get; set; }
    public Guid? DisqualificationReasonId { get; set; }
    public LostReason? DisqualificationReason { get; set; }
    public string? DisqualificationNote { get; set; }
    public NurtureStateType NurtureState { get; set; }
    public Guid? CurrentNurtureJourneyId { get; set; }

    // Miscellaneous
    public Guid? CaptureFormId { get; set; }
    public bool IsSpam { get; set; }
    public decimal? SourceRoi { get; set; }
    public Guid? MergedIntoLeadId { get; set; }
    public Lead? MergedIntoLead { get; set; }

    public ICollection<LeadOwnershipHistory> OwnershipHistories { get; set; } = [];

    public void SetNotes(string? notes) => Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
}
