using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Gress;
using YoutubeDownloader.Core.Utils;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;

namespace YoutubeDownloader.Core.Downloading;

/// <summary>
/// 视频下载器类，负责从YouTube获取视频信息并下载视频
/// </summary>
/// <param name="initialCookies">初始Cookie列表，用于身份验证和访问限制内容</param>
public class VideoDownloader(IReadOnlyList<Cookie>? initialCookies = null)
{
    /// <summary>
    /// YouTube客户端实例，用于与YouTube API交互
    /// </summary>
    private readonly YoutubeClient _youtube = new(Http.Client, initialCookies ?? []);

    /// <summary>
    /// 获取指定视频的所有可用下载选项
    /// </summary>
    /// <param name="videoId">YouTube视频ID</param>
    /// <param name="includeLanguageSpecificAudioStreams">是否包含特定语言的音频流</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>可用的下载选项列表</returns>
    public async Task<IReadOnlyList<VideoDownloadOption>> GetDownloadOptionsAsync(
        VideoId videoId,
        bool includeLanguageSpecificAudioStreams = true,
        CancellationToken cancellationToken = default
    )
    {
        var manifest = await _youtube.Videos.Streams.GetManifestAsync(videoId, cancellationToken);
        return VideoDownloadOption.ResolveAll(manifest, includeLanguageSpecificAudioStreams);
    }

    /// <summary>
    /// 根据用户偏好获取最佳的下载选项
    /// </summary>
    /// <param name="videoId">YouTube视频ID</param>
    /// <param name="preference">视频下载偏好设置</param>
    /// <param name="includeLanguageSpecificAudioStreams">是否包含特定语言的音频流</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>最佳的下载选项</returns>
    /// <exception cref="InvalidOperationException">当找不到合适的下载选项时抛出</exception>
    public async Task<VideoDownloadOption> GetBestDownloadOptionAsync(
        VideoId videoId,
        VideoDownloadPreference preference,
        bool includeLanguageSpecificAudioStreams = true,
        CancellationToken cancellationToken = default
    )
    {
        var options = await GetDownloadOptionsAsync(
            videoId,
            includeLanguageSpecificAudioStreams,
            cancellationToken
        );

        return preference.TryGetBestOption(options)
            ?? throw new InvalidOperationException("No suitable download option found.");
    }

    /// <summary>
    /// 下载视频到指定文件路径
    /// </summary>
    /// <param name="filePath">保存视频的文件路径</param>
    /// <param name="video">要下载的视频信息</param>
    /// <param name="downloadOption">下载选项</param>
    /// <param name="includeSubtitles">是否包含字幕</param>
    /// <param name="progress">下载进度报告器</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步下载操作的任务</returns>
    public async Task DownloadVideoAsync(
        string filePath,
        IVideo video,
        VideoDownloadOption downloadOption,
        bool includeSubtitles = true,
        IProgress<Percentage>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        // 如果需要并且容器支持，则包含字幕
        var trackInfos = new List<ClosedCaptionTrackInfo>();
        if (includeSubtitles && !downloadOption.Container.IsAudioOnly)
        {
            var manifest = await _youtube.Videos.ClosedCaptions.GetManifestAsync(
                video.Id,
                cancellationToken
            );

            trackInfos.AddRange(manifest.Tracks);
        }

        // 确保目标目录存在
        var dirPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(dirPath))
            Directory.CreateDirectory(dirPath);

        // 执行下载并转换
        await _youtube.Videos.DownloadAsync(
            downloadOption.StreamInfos,
            trackInfos,
            new ConversionRequestBuilder(filePath)
                .SetFFmpegPath(FFmpeg.TryGetCliFilePath() ?? "ffmpeg")
                .SetContainer(downloadOption.Container)
                .SetPreset(ConversionPreset.Medium)
                .Build(),
            progress?.ToDoubleBased(),
            cancellationToken
        );
    }
}
