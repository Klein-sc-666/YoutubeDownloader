using System;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeDownloader.Utils.Extensions;

/// <summary>
/// 可释放对象扩展工具类，提供处理IDisposable对象集合的辅助方法
/// </summary>
internal static class DisposableExtensions
{
    /// <summary>
    /// 释放集合中的所有可释放对象
    /// 捕获并收集释放过程中发生的所有异常，最后以聚合异常形式抛出
    /// </summary>
    /// <param name="disposables">要释放的对象集合</param>
    /// <exception cref="AggregateException">如果释放过程中发生一个或多个异常</exception>
    public static void DisposeAll(this IEnumerable<IDisposable> disposables)
    {
        var exceptions = default(List<Exception>);

        // 遍历所有可释放对象并尝试释放它们
        foreach (var disposable in disposables)
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception ex)
            {
                // 收集释放过程中发生的异常
                (exceptions ??= []).Add(ex);
            }
        }

        // 如果有异常发生，则抛出聚合异常
        if (exceptions?.Any() == true)
            throw new AggregateException(exceptions);
    }
}
