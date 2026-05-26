// <copyright file="GetCustomerManagementDataQualityIssuesQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.DTOs.DataQuality;

namespace NetMetric.CRM.CustomerManagement.Application.Features.DataQuality.Queries.GetCustomerManagementDataQualityIssues;

public sealed class GetCustomerManagementDataQualityIssuesQuery : IRequest<IReadOnlyList<DataQualityIssueDto>>
{
    public int Take { get; init; } = 200;
}
