using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Core.Utils;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Resolving;

/// <summary>
/// 查询解析器类，负责解析用户输入的查询并获取相应的YouTube内容
/// </summary>
/// <param name="initialCookies">初始Cookie列表，用于身份验证和访问限制内容</param>
public class QueryResolver(IReadOnlyList<Cookie>? initialCookies = null)
{
    /// <summary>
    /// YouTube客户端实例，用于与YouTube API交互
    /// </summary>
    private readonly YoutubeClient _youtube = new(Http.Client, initialCookies ?? []);

    /// <summary>
    /// 是否已通过Cookie进行身份验证
    /// </summary>
    private readonly bool _isAuthenticated = initialCookies?.Any() == true;

    /// <summary>
    /// 尝试将查询解析为播放列表
    /// </summary>
    /// <param name="query">用户输入的查询字符串</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>播放列表查询结果，如果查询不是有效的播放列表则返回null</returns>
    private async Task<QueryResult?> TryResolvePlaylistAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        if (PlaylistId.TryParse(query) is not { } playlistId)
            return null;

        // 如果用户未登录，跳过个人系统播放列表（如稍后观看、喜欢的视频等）
        var isPersonalSystemPlaylist =
            playlistId == "WL" || playlistId == "LL" || playlistId == "LM";

        if (isPersonalSystemPlaylist && !_isAuthenticated)
            return null;

        var playlist = await _youtube.Playlists.GetAsync(playlistId, cancellationToken);
        var videos = await _youtube.Playlists.GetVideosAsync(playlistId, cancellationToken);

        return new QueryResult(QueryResultKind.Playlist, $"Playlist: {playlist.Title}", videos);
    }

    /// <summary>
    /// 尝试将查询解析为单个视频
    /// </summary>
    /// <param name="query">用户输入的查询字符串</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>视频查询结果，如果查询不是有效的视频ID则返回null</returns>
    private async Task<QueryResult?> TryResolveVideoAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        if (VideoId.TryParse(query) is not { } videoId)
            return null;

        var video = await _youtube.Videos.GetAsync(videoId, cancellationToken);
        return new QueryResult(QueryResultKind.Video, video.Title, [video]);
    }

    /// <summary>
    /// 尝试将查询解析为频道
    /// </summary>
    /// <param name="query">用户输入的查询字符串</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>频道查询结果，如果查询不是有效的频道标识符则返回null</returns>
    private async Task<QueryResult?> TryResolveChannelAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        if (ChannelId.TryParse(query) is { } channelId)
        {
            var channel = await _youtube.Channels.GetAsync(channelId, cancellationToken);
            var videos = await _youtube.Channels.GetUploadsAsync(channelId, cancellationToken);

            return new QueryResult(QueryResultKind.Channel, $"Channel: {channel.Title}", videos);
        }

        if (ChannelHandle.TryParse(query) is { } channelHandle)
        {
            var channel = await _youtube.Channels.GetByHandleAsync(
                channelHandle,
                cancellationToken
            );

            var videos = await _youtube.Channels.GetUploadsAsync(channel.Id, cancellationToken);

            return new QueryResult(QueryResultKind.Channel, $"Channel: {channel.Title}", videos);
        }

        if (UserName.TryParse(query) is { } userName)
        {
            var channel = await _youtube.Channels.GetByUserAsync(userName, cancellationToken);
            var videos = await _youtube.Channels.GetUploadsAsync(channel.Id, cancellationToken);

            return new QueryResult(QueryResultKind.Channel, $"Channel: {channel.Title}", videos);
        }

        if (ChannelSlug.TryParse(query) is { } channelSlug)
        {
            var channel = await _youtube.Channels.GetBySlugAsync(channelSlug, cancellationToken);
            var videos = await _youtube.Channels.GetUploadsAsync(channel.Id, cancellationToken);

            return new QueryResult(QueryResultKind.Channel, $"Channel: {channel.Title}", videos);
        }

        return null;
    }

    /// <summary>
    /// 将查询解析为搜索结果
    /// </summary>
    /// <param name="query">搜索关键词</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>搜索查询结果</returns>
    private async Task<QueryResult> ResolveSearchAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        var videos = await _youtube
            .Search.GetVideosAsync(query, cancellationToken)
            .CollectAsync(20);

        return new QueryResult(QueryResultKind.Search, $"Search: {query}", videos);
    }

    /// <summary>
    /// 解析用户输入的查询
    /// </summary>
    /// <param name="query">用户输入的查询字符串</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>查询结果</returns>
    /// <remarks>
    /// 解析顺序：
    /// 1. 如果查询以问号开头，直接作为搜索查询处理
    /// 2. 尝试解析为播放列表
    /// 3. 尝试解析为视频
    /// 4. 尝试解析为频道
    /// 5. 如果以上都失败，作为搜索查询处理
    /// </remarks>
    public async Task<QueryResult> ResolveAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        // 如果查询以问号开头，直接作为搜索查询处理
        if (query.StartsWith('?'))
            return await ResolveSearchAsync(query[1..], cancellationToken);

        return await TryResolvePlaylistAsync(query, cancellationToken)
            ?? await TryResolveVideoAsync(query, cancellationToken)
            ?? await TryResolveChannelAsync(query, cancellationToken)
            ?? await ResolveSearchAsync(query, cancellationToken);
    }
}
