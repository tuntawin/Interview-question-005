using System.Data;
using Microsoft.EntityFrameworkCore;
using QueueApp.Api.Data;
using QueueApp.Api.Models;

namespace QueueApp.Api.Services;

public class QueueService : IQueueService
{
    private readonly QueueDbContext _dbContext;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public QueueService(QueueDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string IndexToQueueCode(int index)
    {
        if (index < 0 || index > 259)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be between 0 and 259.");
        }

        char letter = (char)('A' + (index / 10));
        int digit = index % 10;
        return $"{letter}{digit}";
    }

    public async Task<QueueGenerateResponse> GenerateNextQueueAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            await using var transaction = _dbContext.Database.IsRelational()
                ? await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
                : null;

            var setting = await _dbContext.QueueSettings
                .FirstOrDefaultAsync(s => s.Id == 1, cancellationToken);

            if (setting == null)
            {
                setting = new QueueSetting
                {
                    Id = 1,
                    CurrentIndex = -1,
                    LastActive = DateTime.UtcNow
                };
                _dbContext.QueueSettings.Add(setting);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            int nextIndex = (setting.CurrentIndex < 0 || setting.CurrentIndex >= 259) ? 0 : setting.CurrentIndex + 1;
            string queueCode = IndexToQueueCode(nextIndex);

            setting.CurrentIndex = nextIndex;
            setting.LastActive = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction != null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            return new QueueGenerateResponse(queueCode, nextIndex, setting.LastActive);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<QueueResetResponse> ResetQueueAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            await using var transaction = _dbContext.Database.IsRelational()
                ? await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
                : null;

            var setting = await _dbContext.QueueSettings
                .FirstOrDefaultAsync(s => s.Id == 1, cancellationToken);

            if (setting == null)
            {
                setting = new QueueSetting
                {
                    Id = 1,
                    CurrentIndex = -1,
                    LastActive = DateTime.UtcNow
                };
                _dbContext.QueueSettings.Add(setting);
            }
            else
            {
                setting.CurrentIndex = -1;
                setting.LastActive = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction != null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            return new QueueResetResponse("Queue has been reset successfully.", -1);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<QueueCurrentResponse> GetCurrentQueueAsync(CancellationToken cancellationToken = default)
    {
        var setting = await _dbContext.QueueSettings
            .FirstOrDefaultAsync(s => s.Id == 1, cancellationToken);

        if (setting == null || setting.CurrentIndex < 0)
        {
            return new QueueCurrentResponse("00", -1, setting?.LastActive);
        }

        string queueCode = IndexToQueueCode(setting.CurrentIndex);
        return new QueueCurrentResponse(queueCode, setting.CurrentIndex, setting.LastActive);
    }
}

