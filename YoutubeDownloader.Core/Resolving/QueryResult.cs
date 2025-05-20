using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Resolving;

/// <summary>
/// 查询结果记录类，包含查询的类型、标题和视频列表
/// </summary>
/// <param name="Kind">查询结果的类型（视频、播放列表、频道等）</param>
/// <param name="Title">查询结果的标题</param>
/// <param name="Videos">查询结果包含的视频列表</param>
public record QueryResult(QueryResultKind Kind, string Title, IReadOnlyList<IVideo> Videos)
{
    /// <summary>
    /// 聚合多个查询结果为一个结果
    /// </summary>
    /// <param name="results">要聚合的查询结果列表</param>
    /// <returns>聚合后的查询结果</returns>
    /// <exception cref="ArgumentException">当传入空列表时抛出</exception>
    public static QueryResult Aggregate(IReadOnlyList<QueryResult> results)
    {
        if (!results.Any())
            throw new ArgumentException("Cannot aggregate empty results.", nameof(results));

        return new QueryResult(
            // 单个查询 -> 继承其类型，多个查询 -> 标记为聚合类型
            results.Count == 1
                ? results.Single().Kind
                : QueryResultKind.Aggregate,
            // 单个查询 -> 继承其标题，多个查询 -> 使用聚合标题
            results.Count == 1
                ? results.Single().Title
                : $"{results.Count} queries",
            // 合并所有视频，按ID去重
            results.SelectMany(q => q.Videos).DistinctBy(v => v.Id).ToArray()
        );
    }
}
