using QueueApp.Api.Models;

namespace QueueApp.Api.Services;

public interface IQueueService
{
    Task<QueueGenerateResponse> GenerateNextQueueAsync(CancellationToken cancellationToken = default);
    Task<QueueResetResponse> ResetQueueAsync(CancellationToken cancellationToken = default);
    string IndexToQueueCode(int index);
}
