namespace YoutubeDownloader.Core.Resolving;

/// <summary>
/// 查询结果类型枚举，用于标识不同来源的查询结果
/// </summary>
public enum QueryResultKind
{
    /// <summary>
    /// 单个视频的查询结果
    /// </summary>
    Video,

    /// <summary>
    /// 播放列表的查询结果
    /// </summary>
    Playlist,

    /// <summary>
    /// 频道的查询结果（通常包含频道上传的视频）
    /// </summary>
    Channel,

    /// <summary>
    /// 搜索查询的结果（通过关键词搜索得到的视频列表）
    /// </summary>
    Search,

    /// <summary>
    /// 聚合结果（多个不同类型查询结果的组合）
    /// </summary>
    Aggregate,
}
