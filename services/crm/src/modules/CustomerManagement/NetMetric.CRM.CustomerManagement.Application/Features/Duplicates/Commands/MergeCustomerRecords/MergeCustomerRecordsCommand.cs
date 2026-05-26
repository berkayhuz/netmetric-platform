// <copyright file="MergeCustomerRecordsCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Duplicates.Commands.MergeCustomerRecords;

public sealed record MergeCustomerRecordsCommand(Guid TargetCustomerId, Guid SourceCustomerId) : IRequest;
