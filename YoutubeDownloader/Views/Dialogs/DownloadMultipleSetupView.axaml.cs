using Avalonia.Interactivity;
using YoutubeDownloader.Framework;
using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.Views.Dialogs;

/// <summary>
/// 批量下载设置视图，用于配置多个视频的下载参数
/// </summary>
public partial class DownloadMultipleSetupView : UserControl<DownloadMultipleSetupViewModel>
{
    /// <summary>
    /// 构造函数，初始化控件
    /// </summary>
    public DownloadMultipleSetupView() => InitializeComponent();

    /// <summary>
    /// 控件加载完成事件处理方法，执行视图模型的初始化命令
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="args">路由事件参数</param>
    private void UserControl_OnLoaded(object? sender, RoutedEventArgs args) =>
        DataContext.InitializeCommand.Execute(null);
}
