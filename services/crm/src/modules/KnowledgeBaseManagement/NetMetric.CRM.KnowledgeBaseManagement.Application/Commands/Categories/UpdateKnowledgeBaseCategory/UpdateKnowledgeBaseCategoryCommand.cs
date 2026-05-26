// <copyright file="UpdateKnowledgeBaseCategoryCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.KnowledgeBaseManagement.Contracts.DTOs;

namespace NetMetric.CRM.KnowledgeBaseManagement.Application.Commands.Categories.UpdateKnowledgeBaseCategory;

public sealed record UpdateKnowledgeBaseCategoryCommand(Guid CategoryId, string Name, string? Description, int SortOrder) : IRequest<KnowledgeBaseCategoryDto>;
