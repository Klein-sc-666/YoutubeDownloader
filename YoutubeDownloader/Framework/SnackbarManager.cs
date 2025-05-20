using System;
using Avalonia.Threading;
using Material.Styles.Controls;
using Material.Styles.Models;

namespace YoutubeDownloader.Framework;

public class SnackbarManager
{
    private readonly TimeSpan _defaultDuration = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 显示一个简单的通知消息
    /// </summary>
    /// <param name="message">要显示的通知消息文本</param>
    /// <param name="duration">通知显示的持续时间，如果为null则使用默认时间</param>
    public void Notify(string message, TimeSpan? duration = null) =>
        SnackbarHost.Post(
            // 创建一个只包含消息的Snackbar模型
            new SnackbarModel(message, duration ?? _defaultDuration),
            null, // 不指定目标，使用默认的Snackbar宿主
            DispatcherPriority.Normal // 使用普通优先级进行UI调度
        );

    /// <summary>
    /// 显示一个带有操作按钮的通知消息
    /// </summary>
    /// <param name="message">要显示的通知消息文本</param>
    /// <param name="actionText">操作按钮上显示的文本</param>
    /// <param name="actionHandler">点击操作按钮时执行的回调方法</param>
    /// <param name="duration">通知显示的持续时间，如果为null则使用默认时间</param>
    public void Notify(
        string message,
        string actionText,
        Action actionHandler,
        TimeSpan? duration = null
    ) =>
        SnackbarHost.Post(
            new SnackbarModel(
                message,
                duration ?? _defaultDuration,
                // 创建一个带有文本和操作的按钮模型
                new SnackbarButtonModel { Text = actionText, Action = actionHandler }
            ),
            null, // 不指定目标，使用默认的Snackbar宿主
            DispatcherPriority.Normal // 使用普通优先级进行UI调度
        );
}
