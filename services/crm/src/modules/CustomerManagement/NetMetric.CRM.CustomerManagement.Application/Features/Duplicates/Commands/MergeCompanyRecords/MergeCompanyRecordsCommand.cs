// <copyright file="MergeCompanyRecordsCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Duplicates.Commands.MergeCompanyRecords;

public sealed record MergeCompanyRecordsCommand(Guid TargetCompanyId, Guid SourceCompanyId) : IRequest;
