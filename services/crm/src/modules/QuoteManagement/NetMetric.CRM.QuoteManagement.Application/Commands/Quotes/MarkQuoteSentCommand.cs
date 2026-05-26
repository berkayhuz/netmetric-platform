// <copyright file="MarkQuoteSentCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.QuoteManagement.Application.Commands.Quotes;

public sealed record MarkQuoteSentCommand(Guid QuoteId, DateTime? SentAt, string? Note, string? RowVersion) : IRequest;
