using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Core.Downloading;

/// <summary>
/// 视频下载偏好设置，定义用户对下载视频的容器格式和质量要求
/// </summary>
/// <param name="PreferredContainer">首选的容器格式</param>
/// <param name="PreferredVideoQuality">首选的视频质量</param>
public record VideoDownloadPreference(
    Container PreferredContainer,
    VideoQualityPreference PreferredVideoQuality
)
{
    /// <summary>
    /// 从可用的下载选项中选择最符合用户偏好的选项
    /// </summary>
    /// <param name="options">可用的下载选项列表</param>
    /// <returns>最佳的下载选项，如果没有找到合适的选项则返回null</returns>
    public VideoDownloadOption? TryGetBestOption(IReadOnlyList<VideoDownloadOption> options)
    {
        // 对于纯音频格式，直接返回匹配容器的第一个选项
        if (PreferredContainer.IsAudioOnly)
            return options.FirstOrDefault(o => o.Container == PreferredContainer);

        // 按视频质量排序
        var orderedOptions = options.OrderBy(o => o.VideoQuality).ToArray();

        // 根据用户的质量偏好选择最佳选项
        var preferredOption = PreferredVideoQuality switch
        {
            // 最高质量 - 选择指定容器中质量最高的
            VideoQualityPreference.Highest => orderedOptions.LastOrDefault(o =>
                o.Container == PreferredContainer
            ),

            // 最高1080p - 选择指定容器中不超过1080p的最高质量
            VideoQualityPreference.UpTo1080p => orderedOptions
                .Where(o => o.VideoQuality?.MaxHeight <= 1080)
                .LastOrDefault(o => o.Container == PreferredContainer),

            // 最高720p - 选择指定容器中不超过720p的最高质量
            VideoQualityPreference.UpTo720p => orderedOptions
                .Where(o => o.VideoQuality?.MaxHeight <= 720)
                .LastOrDefault(o => o.Container == PreferredContainer),

            // 最高480p - 选择指定容器中不超过480p的最高质量
            VideoQualityPreference.UpTo480p => orderedOptions
                .Where(o => o.VideoQuality?.MaxHeight <= 480)
                .LastOrDefault(o => o.Container == PreferredContainer),

            // 最高360p - 选择指定容器中不超过360p的最高质量
            VideoQualityPreference.UpTo360p => orderedOptions
                .Where(o => o.VideoQuality?.MaxHeight <= 360)
                .LastOrDefault(o => o.Container == PreferredContainer),

            // 最低质量 - 选择指定容器中质量最低的
            VideoQualityPreference.Lowest => orderedOptions.LastOrDefault(o =>
                o.Container == PreferredContainer
            ),

            // 未知偏好 - 抛出异常
            _ => throw new InvalidOperationException(
                $"Unknown video quality preference '{PreferredVideoQuality}'."
            ),
        };

        // 如果没有找到完全匹配的选项，返回指定容器的第一个选项
        return preferredOption
            ?? orderedOptions.FirstOrDefault(o => o.Container == PreferredContainer);
    }
}
