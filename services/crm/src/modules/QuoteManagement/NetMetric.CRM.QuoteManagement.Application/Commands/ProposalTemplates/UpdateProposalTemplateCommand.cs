// <copyright file="UpdateProposalTemplateCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.QuoteManagement.Contracts.DTOs;

namespace NetMetric.CRM.QuoteManagement.Application.Commands.ProposalTemplates;

public sealed record UpdateProposalTemplateCommand(Guid TemplateId, string Name, string? SubjectTemplate, string BodyTemplate, bool IsDefault, bool IsActive, string? Notes) : IRequest<ProposalTemplateDto>;
