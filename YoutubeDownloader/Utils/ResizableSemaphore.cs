using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeDownloader.Utils;

/// <summary>
/// 可调整大小的信号量，与普通信号量类似，但允许在运行时动态调整最大计数
/// 用于控制对资源的并发访问，同时支持动态调整并发度
/// </summary>
internal partial class ResizableSemaphore : IDisposable
{
    /// <summary>
    /// 用于线程同步的锁对象
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// 等待获取信号量的任务队列
    /// </summary>
    private readonly Queue<TaskCompletionSource> _waiters = new();

    /// <summary>
    /// 用于取消操作的取消令牌源
    /// </summary>
    private readonly CancellationTokenSource _cts = new();

    /// <summary>
    /// 标记信号量是否已被释放
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// 信号量的最大计数，默认为最大整数值
    /// </summary>
    private int _maxCount = int.MaxValue;

    /// <summary>
    /// 当前已分配的计数
    /// </summary>
    private int _count;

    /// <summary>
    /// 获取或设置信号量的最大计数
    /// 设置新值时会自动刷新等待队列
    /// </summary>
    public int MaxCount
    {
        get
        {
            lock (_lock)
            {
                return _maxCount;
            }
        }
        set
        {
            lock (_lock)
            {
                _maxCount = value;
                Refresh();
            }
        }
    }

    /// <summary>
    /// 刷新等待队列，尝试为等待的任务分配信号量
    /// </summary>
    private void Refresh()
    {
        lock (_lock)
        {
            // 在最大计数允许的情况下，为等待的任务提供访问权
            while (_count < MaxCount && _waiters.TryDequeue(out var waiter))
            {
                // 如果等待者已经被完成（可能是由于取消），则不增加计数
                if (waiter.TrySetResult())
                    _count++;
            }
        }
    }

    /// <summary>
    /// 异步获取信号量访问权
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示获取的访问权的可释放对象</returns>
    /// <exception cref="ObjectDisposedException">如果信号量已被释放</exception>
    public async Task<IDisposable> AcquireAsync(CancellationToken cancellationToken = default)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(GetType().Name);

        var waiter = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        await using (_cts.Token.Register(() => waiter.TrySetCanceled(_cts.Token)))
        await using (cancellationToken.Register(() => waiter.TrySetCanceled(cancellationToken)))
        {
            // 将等待者添加到队列
            lock (_lock)
            {
                _waiters.Enqueue(waiter);
                Refresh();
            }

            // 等待直到此等待者获得访问权
            await waiter.Task;

            return new AcquiredAccess(this);
        }
    }

    /// <summary>
    /// 释放一个信号量计数
    /// </summary>
    private void Release()
    {
        lock (_lock)
        {
            _count--;
            Refresh();
        }
    }

    /// <summary>
    /// 释放信号量资源
    /// </summary>
    public void Dispose()
    {
        if (!_isDisposed)
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        _isDisposed = true;
    }
}

/// <summary>
/// ResizableSemaphore的部分类定义，包含内部辅助类
/// </summary>
internal partial class ResizableSemaphore
{
    /// <summary>
    /// 表示已获取的信号量访问权
    /// 当此对象被释放时，会自动释放对应的信号量计数
    /// </summary>
    /// <param name="semaphore">所属的信号量对象</param>
    private class AcquiredAccess(ResizableSemaphore semaphore) : IDisposable
    {
        /// <summary>
        /// 标记访问权是否已被释放
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// 释放访问权，归还信号量计数
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                semaphore.Release();
            }

            _isDisposed = true;
        }
    }
}
