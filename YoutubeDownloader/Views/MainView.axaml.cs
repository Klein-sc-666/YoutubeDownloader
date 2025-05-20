using Avalonia.Interactivity;
using YoutubeDownloader.Framework;
using YoutubeDownloader.ViewModels;

namespace YoutubeDownloader.Views;

/// <summary>
/// 应用程序主窗口视图，负责显示主界面并处理初始化逻辑
/// </summary>
public partial class MainView : Window<MainViewModel>
{
    /// <summary>
    /// 构造函数，初始化窗口组件
    /// </summary>
    public MainView() => InitializeComponent();

    /// <summary>
    /// 对话框宿主加载完成事件处理方法，执行视图模型的初始化命令
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="args">路由事件参数</param>
    private void DialogHost_OnLoaded(object? sender, RoutedEventArgs args) =>
        DataContext.InitializeCommand.Execute(null);
}
