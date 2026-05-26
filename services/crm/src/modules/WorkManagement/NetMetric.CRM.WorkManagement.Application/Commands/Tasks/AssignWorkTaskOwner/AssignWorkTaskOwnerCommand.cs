using MediatR;
using NetMetric.CRM.WorkManagement.Contracts.DTOs;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Tasks.AssignWorkTaskOwner;

public sealed record AssignWorkTaskOwnerCommand(Guid TaskId, Guid? OwnerUserId) : IRequest<WorkTaskDto?>;
