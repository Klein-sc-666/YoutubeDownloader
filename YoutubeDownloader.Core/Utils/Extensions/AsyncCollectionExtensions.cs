using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace YoutubeDownloader.Core.Utils.Extensions;

/// <summary>
/// 异步集合扩展类，提供处理异步枚举的辅助方法
/// </summary>
public static class AsyncCollectionExtensions
{
    /// <summary>
    /// 将异步枚举收集到只读列表中
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="asyncEnumerable">异步枚举对象</param>
    /// <returns>包含所有元素的只读列表</returns>
    private static async ValueTask<IReadOnlyList<T>> CollectAsync<T>(
        this IAsyncEnumerable<T> asyncEnumerable
    )
    {
        var list = new List<T>();

        await foreach (var i in asyncEnumerable)
            list.Add(i);

        return list;
    }

    /// <summary>
    /// 为异步枚举提供GetAwaiter方法，使其可以直接用于await表达式
    /// 这允许使用 "await asyncEnumerable" 语法而不需要显式调用ToListAsync等方法
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="asyncEnumerable">异步枚举对象</param>
    /// <returns>值任务等待器，完成后返回只读列表</returns>
    public static ValueTaskAwaiter<IReadOnlyList<T>> GetAwaiter<T>(
        this IAsyncEnumerable<T> asyncEnumerable
    ) => asyncEnumerable.CollectAsync().GetAwaiter();
}
