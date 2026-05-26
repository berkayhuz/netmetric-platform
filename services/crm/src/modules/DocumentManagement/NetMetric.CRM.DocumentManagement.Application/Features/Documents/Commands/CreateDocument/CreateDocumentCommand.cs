// <copyright file="CreateDocumentCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.DocumentManagement.Contracts.DTOs;

namespace NetMetric.CRM.DocumentManagement.Application.Features.Documents.Commands.CreateDocument;

public sealed record CreateDocumentCommand(string Name, string ContentType, long SizeBytes) : IRequest<Guid>;
