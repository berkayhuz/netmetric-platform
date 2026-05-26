using MediatR;
using NetMetric.CRM.WorkManagement.Contracts.DTOs;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Tasks.UpdateWorkTaskDueDate;

public sealed record UpdateWorkTaskDueDateCommand(Guid TaskId, DateTime DueAtUtc) : IRequest<WorkTaskDto?>;
