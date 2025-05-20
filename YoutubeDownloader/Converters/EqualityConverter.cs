using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace YoutubeDownloader.Converters;

/// <summary>
/// 相等性转换器，用于在XAML绑定中比较两个值是否相等
/// </summary>
/// <param name="isInverted">是否反转比较结果，true表示检查不相等，false表示检查相等</param>
public class EqualityConverter(bool isInverted) : IValueConverter
{
    /// <summary>
    /// 相等性检查的静态实例，用于检查两个值是否相等
    /// </summary>
    public static EqualityConverter Equality { get; } = new(false);

    /// <summary>
    /// 不等性检查的静态实例，用于检查两个值是否不相等
    /// </summary>
    public static EqualityConverter IsNotEqual { get; } = new(true);

    /// <summary>
    /// 将源值与参数值进行相等性比较
    /// </summary>
    /// <param name="value">要比较的源值</param>
    /// <param name="targetType">目标类型</param>
    /// <param name="parameter">要与源值比较的参数值</param>
    /// <param name="culture">当前文化信息</param>
    /// <returns>如果比较结果与isInverted设置一致则返回true，否则返回false</returns>
    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => EqualityComparer<object>.Default.Equals(value, parameter) != isInverted;

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
