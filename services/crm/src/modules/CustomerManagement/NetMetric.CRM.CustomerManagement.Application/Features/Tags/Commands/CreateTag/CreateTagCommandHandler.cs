// <copyright file="CreateTagCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Tags;
using NetMetric.CRM.Tagging;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Tags.Commands.CreateTag;

public sealed class CreateTagCommandHandler(ICustomerManagementDbContext dbContext) : IRequestHandler<CreateTagCommand, TagDto>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;

    public async Task<TagDto> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.Trim();
        var existing = await _dbContext.Set<Tag>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Name == normalizedName, cancellationToken);

        if (existing is not null)
        {
            return new TagDto
            {
                Id = existing.Id,
                Name = existing.Name,
                ColorHex = existing.ColorHex,
                Description = existing.Description
            };
        }

        var tag = new Tag
        {
            Name = normalizedName,
            ColorHex = string.IsNullOrWhiteSpace(request.ColorHex) ? null : request.ColorHex.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim()
        };

        await _dbContext.Set<Tag>().AddAsync(tag, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            ColorHex = tag.ColorHex,
            Description = tag.Description
        };
    }
}
