using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using YoutubeDownloader.Framework;
using YoutubeDownloader.ViewModels.Components;

namespace YoutubeDownloader.Views.Components;

public partial class DashboardView : UserControl<DashboardViewModel>
{
    /// <summary>
    /// 构造函数，初始化控件并设置事件处理
    /// </summary>
    public DashboardView()
    {
        InitializeComponent();

        // 使用隧道策略绑定事件，以处理参与文本输入的按键
        QueryTextBox.AddHandler(KeyDownEvent, QueryTextBox_OnKeyDown, RoutingStrategies.Tunnel);
    }

    /// <summary>
    /// 控件加载完成时的事件处理方法，使查询文本框获得焦点
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="args">路由事件参数</param>
    private void UserControl_OnLoaded(object? sender, RoutedEventArgs args) => QueryTextBox.Focus();

    /// <summary>
    /// 查询文本框按键事件处理方法
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="args">按键事件参数</param>
    private void QueryTextBox_OnKeyDown(object? sender, KeyEventArgs args)
    {
        // 当按下Enter键且没有同时按下Shift键时，执行默认按钮命令
        // 而不是添加新行
        if (args.Key == Key.Enter && args.KeyModifiers != KeyModifiers.Shift)
        {
            args.Handled = true;
            ProcessQueryButton.Command?.Execute(ProcessQueryButton.CommandParameter);
        }
    }

    /// <summary>
    /// 状态文本块鼠标释放事件处理方法，用于复制错误信息
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="args">指针释放事件参数</param>
    private void StatusTextBlock_OnPointerReleased(object sender, PointerReleasedEventArgs args)
    {
        if (sender is IDataContextProvider { DataContext: DownloadViewModel dataContext })
            dataContext.CopyErrorMessageCommand.Execute(null);
    }
}
