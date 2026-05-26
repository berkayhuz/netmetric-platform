using MediatR;
using NetMetric.CRM.WorkManagement.Contracts.DTOs;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Tasks.ReopenWorkTask;

public sealed record ReopenWorkTaskCommand(Guid TaskId) : IRequest<WorkTaskDto?>;
