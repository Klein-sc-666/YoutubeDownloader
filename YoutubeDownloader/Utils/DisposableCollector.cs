using System;
using System.Collections.Generic;
using YoutubeDownloader.Utils.Extensions;

namespace YoutubeDownloader.Utils;

/// <summary>
/// 可释放资源收集器，用于管理多个IDisposable对象的生命周期
/// 可以收集多个需要释放的资源，并在自身被释放时统一处理它们
/// </summary>
internal class DisposableCollector : IDisposable
{
    /// <summary>
    /// 用于线程同步的锁对象
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// 存储所有需要被管理的可释放资源
    /// </summary>
    private readonly List<IDisposable> _items = [];

    /// <summary>
    /// 添加一个可释放资源到收集器中
    /// </summary>
    /// <param name="item">要添加的可释放资源</param>
    public void Add(IDisposable item)
    {
        lock (_lock)
        {
            _items.Add(item);
        }
    }

    /// <summary>
    /// 释放所有收集的资源并清空集合
    /// 使用线程锁确保线程安全
    /// </summary>
    public void Dispose()
    {
        lock (_lock)
        {
            _items.DisposeAll();
            _items.Clear();
        }
    }
}
