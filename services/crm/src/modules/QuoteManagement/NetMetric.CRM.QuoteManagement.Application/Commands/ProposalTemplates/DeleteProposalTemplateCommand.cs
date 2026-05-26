// <copyright file="DeleteProposalTemplateCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.QuoteManagement.Application.Commands.ProposalTemplates;

public sealed record DeleteProposalTemplateCommand(Guid TemplateId) : IRequest;
