using MediatR;
using NetMetric.CRM.WorkManagement.Contracts.DTOs;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Tasks.UpdateWorkTask;

public sealed record UpdateWorkTaskCommand(Guid TaskId, string Title, string Description, int Priority) : IRequest<WorkTaskDto?>;
