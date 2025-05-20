using System;

namespace YoutubeDownloader.Core.Downloading;

/// <summary>
/// 视频质量偏好枚举，用于指定用户期望的视频下载质量
/// </summary>
public enum VideoQualityPreference
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// 最低质量 - 选择可用的最低分辨率
    /// </summary>
    Lowest,

    /// <summary>
    /// 最高不超过360p的质量
    /// </summary>
    UpTo360p,

    /// <summary>
    /// 最高不超过480p的质量
    /// </summary>
    UpTo480p,

    /// <summary>
    /// 最高不超过720p的质量
    /// </summary>
    UpTo720p,

    /// <summary>
    /// 最高不超过1080p的质量
    /// </summary>
    UpTo1080p,

    /// <summary>
    /// 最高质量 - 选择可用的最高分辨率
    /// </summary>
    Highest,
    // ReSharper restore InconsistentNaming
}

/// <summary>
/// 视频质量偏好的扩展方法类
/// </summary>
public static class VideoQualityPreferenceExtensions
{
    /// <summary>
    /// 获取视频质量偏好的显示名称，用于在用户界面中展示
    /// </summary>
    /// <param name="preference">视频质量偏好枚举值</param>
    /// <returns>对应的用户友好的显示名称</returns>
    /// <exception cref="ArgumentOutOfRangeException">当提供的枚举值不在预定义范围内时抛出</exception>
    public static string GetDisplayName(this VideoQualityPreference preference) =>
        preference switch
        {
            VideoQualityPreference.Lowest => "Lowest quality",
            VideoQualityPreference.UpTo360p => "≤ 360p",
            VideoQualityPreference.UpTo480p => "≤ 480p",
            VideoQualityPreference.UpTo720p => "≤ 720p",
            VideoQualityPreference.UpTo1080p => "≤ 1080p",
            VideoQualityPreference.Highest => "Highest quality",
            _ => throw new ArgumentOutOfRangeException(nameof(preference)),
        };
}
