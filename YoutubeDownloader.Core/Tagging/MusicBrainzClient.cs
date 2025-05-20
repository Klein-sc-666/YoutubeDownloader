using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using JsonExtensions.Http;
using JsonExtensions.Reading;
using YoutubeDownloader.Core.Utils;

namespace YoutubeDownloader.Core.Tagging;

/// <summary>
/// MusicBrainz客户端类，用于查询MusicBrainz API获取音乐元数据
/// </summary>
internal class MusicBrainzClient
{
    /// <summary>
    /// 请求限流锁，限制每秒最多4个请求，以符合MusicBrainz API的使用政策
    /// </summary>
    private readonly ThrottleLock _throttleLock = new(TimeSpan.FromSeconds(1.0 / 4));

    /// <summary>
    /// 根据查询文本搜索音乐记录
    /// </summary>
    /// <param name="query">搜索查询文本</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>匹配的音乐记录异步枚举</returns>
    public async IAsyncEnumerable<MusicBrainzRecording> SearchRecordingsAsync(
        string query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        // 构建MusicBrainz API请求URL
        var url =
            "https://musicbrainz.org/ws/2/recording/"
            + "?version=2"
            + "&fmt=json"
            + "&dismax=true" // 使用DisMax查询解析器，提供更好的相关性排序
            + "&limit=100" // 限制返回结果数量
            + $"&query={Uri.EscapeDataString(query)}";

        // 等待限流锁，确保不超过API请求频率限制
        await _throttleLock.WaitAsync(cancellationToken);
        // 发送请求并获取JSON响应
        var json = await Http.Client.GetJsonAsync(url, cancellationToken);

        // 获取recordings数组，如果不存在则使用空枚举
        var recordingsJson =
            json.GetPropertyOrNull("recordings")?.EnumerateArrayOrNull() ?? default;

        // 遍历每个recording记录
        foreach (var recordingJson in recordingsJson)
        {
            // 提取艺术家名称
            var artist = recordingJson
                .GetPropertyOrNull("artist-credit")
                ?.EnumerateArrayOrNull()
                ?.FirstOrDefault()
                .GetPropertyOrNull("name")
                ?.GetNonWhiteSpaceStringOrNull();

            // 如果没有有效的艺术家名称，跳过此记录
            if (string.IsNullOrWhiteSpace(artist))
                continue;

            // 提取艺术家排序名称
            var artistSort = recordingJson
                .GetPropertyOrNull("artist-credit")
                ?.EnumerateArrayOrNull()
                ?.FirstOrDefault()
                .GetPropertyOrNull("artist")
                ?.GetPropertyOrNull("sort-name")
                ?.GetNonWhiteSpaceStringOrNull();

            // 提取标题
            var title = recordingJson.GetPropertyOrNull("title")?.GetNonWhiteSpaceStringOrNull();

            // 如果没有有效的标题，跳过此记录
            if (string.IsNullOrWhiteSpace(title))
                continue;

            // 提取专辑名称（从第一个发行版中）
            var album = recordingJson
                .GetPropertyOrNull("releases")
                ?.EnumerateArrayOrNull()
                ?.FirstOrDefault()
                .GetPropertyOrNull("title")
                ?.GetNonWhiteSpaceStringOrNull();

            // 创建并返回音乐记录对象
            yield return new MusicBrainzRecording(artist, artistSort, title, album);
        }
    }
}
