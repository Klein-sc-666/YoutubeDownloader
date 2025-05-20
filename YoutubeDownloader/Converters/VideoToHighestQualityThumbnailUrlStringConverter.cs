using System;
using System.Globalization;
using Avalonia.Data.Converters;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Converters;

/// <summary>
/// 视频到最高质量缩略图URL转换器，用于从视频对象中提取最高分辨率的缩略图URL
/// </summary>
public class VideoToHighestQualityThumbnailUrlStringConverter : IValueConverter
{
    /// <summary>
    /// 转换器的单例实例
    /// </summary>
    public static VideoToHighestQualityThumbnailUrlStringConverter Instance { get; } = new();

    /// <summary>
    /// 将视频对象转换为其最高质量缩略图的URL
    /// </summary>
    /// <param name="value">要转换的视频对象</param>
    /// <param name="targetType">目标类型</param>
    /// <param name="parameter">转换参数（未使用）</param>
    /// <param name="culture">当前文化信息</param>
    /// <returns>视频的最高分辨率缩略图URL，如果无法获取则返回null</returns>
    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => value is IVideo video ? video.Thumbnails.TryGetWithHighestResolution()?.Url : null;

    /// <summary>
    /// 不支持反向转换
    /// </summary>
    /// <exception cref="NotSupportedException">当尝试进行反向转换时抛出</exception>
    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
