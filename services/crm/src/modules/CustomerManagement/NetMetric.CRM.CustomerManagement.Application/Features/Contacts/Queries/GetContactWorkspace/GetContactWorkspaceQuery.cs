// <copyright file="GetContactWorkspaceQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Contacts;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Contacts.Queries.GetContactWorkspace;

public sealed record GetContactWorkspaceQuery(Guid ContactId) : IRequest<ContactWorkspaceDto>;
