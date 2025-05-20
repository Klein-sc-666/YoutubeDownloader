using System.Collections.Generic;

namespace YoutubeDownloader.Core.Utils.Extensions;

/// <summary>
/// 集合扩展类，提供处理集合的辅助方法
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// 将一个集合中的所有元素添加到另一个集合中
    /// 类似于List<T>的AddRange方法，但适用于所有实现ICollection<T>接口的集合
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="source">目标集合，将接收新元素</param>
    /// <param name="items">要添加的元素集合</param>
    public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> items)
    {
        foreach (var i in items)
            source.Add(i);
    }
}
