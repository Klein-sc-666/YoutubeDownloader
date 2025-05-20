using System;
using System.Globalization;
using Avalonia.Data.Converters;
using YoutubeDownloader.Core.Downloading;

namespace YoutubeDownloader.Converters;

/// <summary>
/// 视频质量偏好转字符串转换器，用于将VideoQualityPreference枚举值转换为用户友好的显示文本
/// </summary>
public class VideoQualityPreferenceToStringConverter : IValueConverter
{
    /// <summary>
    /// 转换器的单例实例
    /// </summary>
    public static VideoQualityPreferenceToStringConverter Instance { get; } = new();

    /// <summary>
    /// 将VideoQualityPreference枚举值转换为对应的显示名称字符串
    /// </summary>
    /// <param name="value">要转换的VideoQualityPreference枚举值</param>
    /// <param name="targetType">目标类型</param>
    /// <param name="parameter">转换参数（未使用）</param>
    /// <param name="culture">当前文化信息</param>
    /// <returns>用户友好的质量偏好显示名称，如果输入无效则返回null</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is VideoQualityPreference preference)
            return preference.GetDisplayName();

        return default(string);
    }

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
