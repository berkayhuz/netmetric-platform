// <copyright file="CreateRevisionCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.QuoteManagement.Contracts.DTOs;

namespace NetMetric.CRM.QuoteManagement.Application.Commands.Quotes;

public sealed record CreateRevisionCommand(Guid QuoteId, string NewQuoteNumber) : IRequest<QuoteDetailDto>;
