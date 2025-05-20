using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;

namespace YoutubeDownloader.Utils.Extensions;

/// <summary>
/// Avalonia框架扩展工具类，提供Avalonia应用程序相关的辅助方法
/// </summary>
internal static class AvaloniaExtensions
{
    /// <summary>
    /// 尝试获取应用程序的主窗口
    /// </summary>
    /// <param name="lifetime">应用程序生命周期对象</param>
    /// <returns>主窗口对象，如果不是桌面应用则返回null</returns>
    public static Window? TryGetMainWindow(this IApplicationLifetime lifetime) =>
        lifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime
            ? desktopLifetime.MainWindow
            : null;

    /// <summary>
    /// 尝试获取应用程序的顶级视图
    /// 首先尝试获取主窗口，如果失败则尝试获取单视图应用的主视图
    /// </summary>
    /// <param name="lifetime">应用程序生命周期对象</param>
    /// <returns>顶级视图对象，如果无法获取则返回null</returns>
    public static TopLevel? TryGetTopLevel(this IApplicationLifetime lifetime) =>
        lifetime.TryGetMainWindow()
        ?? (lifetime as ISingleViewApplicationLifetime)?.MainView?.GetVisualRoot() as TopLevel;

    /// <summary>
    /// 尝试关闭应用程序
    /// 根据应用程序类型选择合适的关闭方法
    /// </summary>
    /// <param name="lifetime">应用程序生命周期对象</param>
    /// <param name="exitCode">退出代码，默认为0</param>
    /// <returns>是否成功启动关闭过程</returns>
    public static bool TryShutdown(this IApplicationLifetime lifetime, int exitCode = 0)
    {
        if (lifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            return desktopLifetime.TryShutdown(exitCode);
        }

        if (lifetime is IControlledApplicationLifetime controlledLifetime)
        {
            controlledLifetime.Shutdown(exitCode);
            return true;
        }

        return false;
    }
}
