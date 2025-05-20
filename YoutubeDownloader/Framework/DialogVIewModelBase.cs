using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace YoutubeDownloader.Framework;

/// <summary>
/// 对话框视图模型基类，提供对话框结果的泛型实现
/// </summary>
/// <typeparam name="T">对话框结果的类型</typeparam>
public abstract partial class DialogViewModelBase<T> : ViewModelBase
{
    /// <summary>
    /// 用于异步等待对话框关闭的任务完成源
    /// </summary>
    private readonly TaskCompletionSource<T> _closeTcs = new(
        TaskCreationOptions.RunContinuationsAsynchronously
    );

    /// <summary>
    /// 对话框的结果值
    /// </summary>
    [ObservableProperty]
    public partial T? DialogResult { get; set; }

    /// <summary>
    /// 关闭对话框并设置结果值
    /// </summary>
    /// <param name="dialogResult">对话框的结果值</param>
    [RelayCommand]
    protected void Close(T dialogResult)
    {
        DialogResult = dialogResult;
        _closeTcs.TrySetResult(dialogResult);
    }

    /// <summary>
    /// 异步等待对话框关闭并获取结果
    /// </summary>
    /// <returns>对话框的结果值</returns>
    public async Task<T> WaitForCloseAsync() => await _closeTcs.Task;
}

/// <summary>
/// 对话框视图模型基类的非泛型版本，使用bool?作为默认结果类型
/// </summary>
public abstract class DialogViewModelBase : DialogViewModelBase<bool?>;
