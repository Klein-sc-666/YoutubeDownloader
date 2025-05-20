using System;
using Avalonia.Controls;

namespace YoutubeDownloader.Framework;

/// <summary>
/// 泛型用户控件基类，提供类型安全的DataContext访问
/// </summary>
/// <typeparam name="TDataContext">数据上下文的类型</typeparam>
public class UserControl<TDataContext> : UserControl
{
    /// <summary>
    /// 获取或设置类型安全的数据上下文
    /// </summary>
    /// <exception cref="InvalidCastException">当数据上下文为null或类型不匹配时抛出</exception>
    public new TDataContext DataContext
    {
        get =>
            base.DataContext is TDataContext dataContext
                ? dataContext
                : throw new InvalidCastException(
                    $"DataContext是null或不是预期的类型'{typeof(TDataContext).FullName}'。"
                );
        set => base.DataContext = value;
    }
}
