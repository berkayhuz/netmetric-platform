// <copyright file="RejectQuoteCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.QuoteManagement.Application.Commands.Quotes;

public sealed record RejectQuoteCommand(Guid QuoteId, string Reason, string? RowVersion) : IRequest;
