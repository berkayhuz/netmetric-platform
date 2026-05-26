// <copyright file="SoftDeleteContactCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Contacts;

public sealed record SoftDeleteContactCommand(Guid ContactId) : IRequest<Unit>;
