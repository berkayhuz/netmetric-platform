using MediatR;
using NetMetric.CRM.WorkManagement.Contracts.DTOs;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Tasks.CompleteWorkTask;

public sealed record CompleteWorkTaskCommand(Guid TaskId, Guid? CompletedByUserId, string? CompletionNote) : IRequest<WorkTaskDto?>;
