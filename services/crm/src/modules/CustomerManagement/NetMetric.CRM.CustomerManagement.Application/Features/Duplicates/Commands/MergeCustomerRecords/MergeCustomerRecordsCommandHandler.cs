// <copyright file="MergeCustomerRecordsCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Services;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Duplicates.Commands.MergeCustomerRecords;

public sealed class MergeCustomerRecordsCommandHandler(ICustomerManagementMergeService mergeService) : IRequestHandler<MergeCustomerRecordsCommand>
{
    private readonly ICustomerManagementMergeService _mergeService = mergeService;

    public async Task Handle(MergeCustomerRecordsCommand request, CancellationToken cancellationToken)
    {
        await _mergeService.MergeCustomersAsync(request.TargetCustomerId, request.SourceCustomerId, cancellationToken);
    }
}
