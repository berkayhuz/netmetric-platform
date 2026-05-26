// <copyright file="QuoteStateMachine.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.QuoteManagement.Application.Common;

public static class QuoteStateMachine
{
    public static bool CanEdit(QuoteStatusType status) => status is QuoteStatusType.Draft or QuoteStatusType.Rejected;
    public static bool CanSubmit(QuoteStatusType status) => status is QuoteStatusType.Draft or QuoteStatusType.Rejected;
    public static bool CanApprove(QuoteStatusType status) => status == QuoteStatusType.Submitted;
    public static bool CanReject(QuoteStatusType status) => status == QuoteStatusType.Submitted;
    public static bool CanSend(QuoteStatusType status) => status == QuoteStatusType.Approved;
    public static bool CanAccept(QuoteStatusType status) => status == QuoteStatusType.Sent;
    public static bool CanDecline(QuoteStatusType status) => status == QuoteStatusType.Sent;
    public static bool CanExpire(QuoteStatusType status) => status == QuoteStatusType.Sent || status == QuoteStatusType.Approved;
}
