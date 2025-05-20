using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeDownloader.Core.Utils.Extensions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Core.Downloading;

/// <summary>
/// 视频下载选项记录类，定义了下载视频时的容器格式、是否仅下载音频以及相关流信息
/// </summary>
/// <param name="Container">视频容器格式，如MP4、WebM等</param>
/// <param name="IsAudioOnly">是否仅下载音频，不包含视频轨道</param>
/// <param name="StreamInfos">下载所需的流信息列表，包含视频流和/或音频流</param>
public partial record VideoDownloadOption(
    Container Container,
    bool IsAudioOnly,
    IReadOnlyList<IStreamInfo> StreamInfos
)
{
    /// <summary>
    /// 视频质量属性，从视频流信息中获取最高质量的视频质量值
    /// 如果是纯音频下载选项，则返回null
    /// </summary>
    public VideoQuality? VideoQuality { get; } =
        StreamInfos.OfType<IVideoStreamInfo>().MaxBy(s => s.VideoQuality)?.VideoQuality;
}

public partial record VideoDownloadOption
{
    /// <summary>
    /// 从流清单中解析所有可用的下载选项
    /// </summary>
    /// <param name="manifest">YouTube视频的流清单，包含所有可用的视频和音频流</param>
    /// <param name="includeLanguageSpecificAudioStreams">是否包含特定语言的音频流，默认为true</param>
    /// <returns>可用的下载选项列表</returns>
    internal static IReadOnlyList<VideoDownloadOption> ResolveAll(
        StreamManifest manifest,
        bool includeLanguageSpecificAudioStreams = true
    )
    {
        // 获取视频和音频组合的下载选项
        // 包括混合流(视频和音频在同一流中)和分离流(视频和音频在不同流中)
        IEnumerable<VideoDownloadOption> GetVideoAndAudioOptions()
        {
            // 获取所有视频流，按质量从高到低排序
            var videoStreamInfos = manifest
                .GetVideoStreams()
                .OrderByDescending(v => v.VideoQuality);

            // 遍历每个视频流，创建相应的下载选项
            foreach (var videoStreamInfo in videoStreamInfos)
            {
                // 混合流 - 视频和音频在同一个流中
                if (videoStreamInfo is MuxedStreamInfo)
                {
                    yield return new VideoDownloadOption(
                        videoStreamInfo.Container,
                        false,
                        [videoStreamInfo]
                    );
                }
                // 分离流 - 需要单独的视频流和音频流
                else
                {
                    // 获取所有音频流，按优先级排序：
                    // 1. 优先选择与视频相同容器格式的音频
                    // 2. 优先选择纯音频流
                    // 3. 按比特率从高到低排序
                    var audioStreamInfos = manifest
                        .GetAudioStreams()
                        // 优先选择与视频相同容器格式的音频
                        .OrderByDescending(s => s.Container == videoStreamInfo.Container)
                        .ThenByDescending(s => s is AudioOnlyStreamInfo)
                        .ThenByDescending(s => s.Bitrate)
                        .ToArray();

                    // 如果允许，优先选择特定语言的音频流
                    var languageSpecificAudioStreamInfos = includeLanguageSpecificAudioStreams
                        ? audioStreamInfos
                            .Where(s => s.AudioLanguage is not null)
                            .DistinctBy(s => s.AudioLanguage)
                            // 默认语言优先，这样它会被编码为输出文件中的第一个音轨
                            .OrderByDescending(s => s.IsAudioLanguageDefault)
                            .ToArray()
                        : [];

                    // 如果存在特定语言的音频流，则包含所有这些流
                    if (languageSpecificAudioStreamInfos.Any())
                    {
                        yield return new VideoDownloadOption(
                            videoStreamInfo.Container,
                            false,
                            [videoStreamInfo, .. languageSpecificAudioStreamInfos]
                        );
                    }
                    // 如果没有特定语言的音频流，则下载单个最佳质量的音频流
                    else
                    {
                        var audioStreamInfo = audioStreamInfos
                            // 优先选择默认语言的音频流(或非语言特定的流)
                            .OrderByDescending(s => s.IsAudioLanguageDefault ?? true)
                            .FirstOrDefault();

                        if (audioStreamInfo is not null)
                        {
                            yield return new VideoDownloadOption(
                                videoStreamInfo.Container,
                                false,
                                [videoStreamInfo, audioStreamInfo]
                            );
                        }
                    }
                }
            }
        }

        // 获取纯音频下载选项
        // 为不同的音频容器格式(WebM、MP3、OGG、MP4)创建下载选项
        IEnumerable<VideoDownloadOption> GetAudioOnlyOptions()
        {
            // WebM基础的纯音频容器(WebM、MP3、OGG)
            {
                // 选择最佳的音频流用于WebM基础的容器
                var audioStreamInfo = manifest
                    .GetAudioStreams()
                    // 优先选择默认语言的音频流(或非语言特定的流)
                    .OrderByDescending(s => s.IsAudioLanguageDefault ?? true)
                    // 优先选择与目标容器相同格式的音频流
                    .ThenByDescending(s => s.Container == Container.WebM)
                    .ThenByDescending(s => s is AudioOnlyStreamInfo)
                    .ThenByDescending(s => s.Bitrate)
                    .FirstOrDefault();

                if (audioStreamInfo is not null)
                {
                    // 创建WebM格式的纯音频下载选项
                    yield return new VideoDownloadOption(Container.WebM, true, [audioStreamInfo]);

                    // 创建MP3格式的纯音频下载选项
                    yield return new VideoDownloadOption(Container.Mp3, true, [audioStreamInfo]);

                    // 创建OGG格式的纯音频下载选项
                    yield return new VideoDownloadOption(
                        new Container("ogg"),
                        true,
                        [audioStreamInfo]
                    );
                }
            }

            // MP4基础的纯音频容器
            {
                // 选择最佳的音频流用于MP4基础的容器
                var audioStreamInfo = manifest
                    .GetAudioStreams()
                    // 优先选择默认语言的音频流(或非语言特定的流)
                    .OrderByDescending(s => s.IsAudioLanguageDefault ?? true)
                    // 优先选择与目标容器相同格式的音频流
                    .ThenByDescending(s => s.Container == Container.Mp4)
                    .ThenByDescending(s => s is AudioOnlyStreamInfo)
                    .ThenByDescending(s => s.Bitrate)
                    .FirstOrDefault();

                if (audioStreamInfo is not null)
                {
                    // 创建MP4格式的纯音频下载选项
                    yield return new VideoDownloadOption(Container.Mp4, true, [audioStreamInfo]);
                }
            }
        }

        // 创建自定义比较器，根据视频质量和容器格式去重
        var comparer = EqualityComparer<VideoDownloadOption>.Create(
            // 比较两个下载选项是否相同(相同的视频质量和容器格式)
            (x, y) => x?.VideoQuality == y?.VideoQuality && x?.Container == y?.Container,
            // 计算下载选项的哈希码
            x => HashCode.Combine(x.VideoQuality, x.Container)
        );

        // 使用HashSet去重，确保不会有重复的下载选项
        var options = new HashSet<VideoDownloadOption>(comparer);

        // 添加所有视频和音频组合的下载选项
        options.AddRange(GetVideoAndAudioOptions());
        // 添加所有纯音频的下载选项
        options.AddRange(GetAudioOnlyOptions());

        // 返回所有可用的下载选项数组
        return options.ToArray();
    }
}
