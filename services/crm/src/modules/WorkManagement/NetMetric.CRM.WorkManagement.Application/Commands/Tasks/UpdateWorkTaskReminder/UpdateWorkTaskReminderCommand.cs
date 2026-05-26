using MediatR;
using NetMetric.CRM.WorkManagement.Contracts.DTOs;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Tasks.UpdateWorkTaskReminder;

public sealed record UpdateWorkTaskReminderCommand(Guid TaskId, DateTime? ReminderAtUtc) : IRequest<WorkTaskDto?>;
