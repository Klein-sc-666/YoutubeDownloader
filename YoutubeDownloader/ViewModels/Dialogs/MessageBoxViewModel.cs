using CommunityToolkit.Mvvm.ComponentModel;
using YoutubeDownloader.Framework;

namespace YoutubeDownloader.ViewModels.Dialogs;

/// <summary>
/// 消息框对话框的视图模型，用于显示各种提示、警告或错误信息
/// </summary>
public partial class MessageBoxViewModel : DialogViewModelBase
{
    /// <summary>
    /// 消息框的标题
    /// </summary>
    [ObservableProperty]
    public partial string? Title { get; set; } = "Title";

    /// <summary>
    /// 消息框的内容
    /// </summary>
    [ObservableProperty]
    public partial string? Message { get; set; } = "Message";

    /// <summary>
    /// 默认按钮（通常是确认按钮）的文本
    /// 当此属性变化时，会同时通知IsDefaultButtonVisible和ButtonsCount属性变化
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDefaultButtonVisible))]
    [NotifyPropertyChangedFor(nameof(ButtonsCount))]
    public partial string? DefaultButtonText { get; set; } = "OK";

    /// <summary>
    /// 取消按钮的文本
    /// 当此属性变化时，会同时通知IsCancelButtonVisible和ButtonsCount属性变化
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCancelButtonVisible))]
    [NotifyPropertyChangedFor(nameof(ButtonsCount))]
    public partial string? CancelButtonText { get; set; } = "Cancel";

    /// <summary>
    /// 判断默认按钮是否可见（文本不为空或空白）
    /// </summary>
    public bool IsDefaultButtonVisible => !string.IsNullOrWhiteSpace(DefaultButtonText);

    /// <summary>
    /// 判断取消按钮是否可见（文本不为空或空白）
    /// </summary>
    public bool IsCancelButtonVisible => !string.IsNullOrWhiteSpace(CancelButtonText);

    /// <summary>
    /// 获取按钮总数，用于布局排列
    /// </summary>
    public int ButtonsCount => (IsDefaultButtonVisible ? 1 : 0) + (IsCancelButtonVisible ? 1 : 0);
}
