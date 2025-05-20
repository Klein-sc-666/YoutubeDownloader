using System;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeDownloader.Core.Utils;

/// <summary>
/// 节流锁，用于限制操作的执行频率，确保两次操作之间有最小的时间间隔
/// </summary>
/// <param name="interval">两次操作之间的最小时间间隔</param>
public class ThrottleLock(TimeSpan interval) : IDisposable
{
    /// <summary>
    /// 信号量，用于控制并发访问
    /// </summary>
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// 上次请求的时间点，初始值设为最小时间，确保首次请求不受限制
    /// </summary>
    private DateTimeOffset _lastRequestInstant = DateTimeOffset.MinValue;

    /// <summary>
    /// 异步等待直到可以执行下一次操作
    /// 如果距离上次操作的时间小于指定的间隔，则会等待剩余时间
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步等待操作的任务</returns>
    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            var timePassedSinceLastRequest = DateTimeOffset.Now - _lastRequestInstant;

            var remainingTime = interval - timePassedSinceLastRequest;
            if (remainingTime > TimeSpan.Zero)
                await Task.Delay(remainingTime, cancellationToken);

            _lastRequestInstant = DateTimeOffset.Now;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 释放资源，处理信号量
    /// </summary>
    public void Dispose() => _semaphore.Dispose();
}
