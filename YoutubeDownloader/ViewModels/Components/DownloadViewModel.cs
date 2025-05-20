using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gress;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Extensions;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Components;

/// <summary>
/// 下载任务视图模型，负责管理单个视频的下载过程和状态
/// </summary>
public partial class DownloadViewModel : ViewModelBase
{
    private readonly ViewModelManager _viewModelManager;
    private readonly DialogManager _dialogManager;

    private readonly DisposableCollector _eventRoot = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private bool _isDisposed;

    /// <summary>
    /// 要下载的YouTube视频
    /// </summary>
    [ObservableProperty]
    public partial IVideo? Video { get; set; }

    /// <summary>
    /// 视频下载选项，包含具体的格式和质量信息
    /// </summary>
    [ObservableProperty]
    public partial VideoDownloadOption? DownloadOption { get; set; }

    /// <summary>
    /// 视频下载偏好，用于在没有具体下载选项时选择最佳格式
    /// </summary>
    [ObservableProperty]
    public partial VideoDownloadPreference? DownloadPreference { get; set; }

    /// <summary>
    /// 下载文件的完整路径
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FileName))]
    public partial string? FilePath { get; set; }

    /// <summary>
    /// 下载任务的当前状态
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCanceledOrFailed))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenFileCommand))]
    public partial DownloadStatus Status { get; set; } = DownloadStatus.Enqueued;

    /// <summary>
    /// 下载失败时的错误信息
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CopyErrorMessageCommand))]
    public partial string? ErrorMessage { get; set; }

    /// <summary>
    /// 初始化下载视图模型
    /// </summary>
    public DownloadViewModel(ViewModelManager viewModelManager, DialogManager dialogManager)
    {
        _viewModelManager = viewModelManager;
        _dialogManager = dialogManager;

        // 监听进度变化以更新进度指示器状态
        _eventRoot.Add(
            Progress.WatchProperty(
                o => o.Current,
                () => OnPropertyChanged(nameof(IsProgressIndeterminate))
            )
        );
    }

    /// <summary>
    /// 获取用于取消下载的令牌
    /// </summary>
    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    /// <summary>
    /// 获取下载文件的文件名（不含路径）
    /// </summary>
    public string? FileName => Path.GetFileName(FilePath);

    /// <summary>
    /// 下载进度容器，用于跟踪和显示下载进度
    /// </summary>
    public ProgressContainer<Percentage> Progress { get; } = new();

    /// <summary>
    /// 指示进度是否为不确定状态
    /// </summary>
    public bool IsProgressIndeterminate => Progress.Current.Fraction is <= 0 or >= 1;

    /// <summary>
    /// 指示下载是否已取消或失败
    /// </summary>
    public bool IsCanceledOrFailed => Status is DownloadStatus.Canceled or DownloadStatus.Failed;

    /// <summary>
    /// 检查是否可以取消下载
    /// </summary>
    private bool CanCancel() => Status is DownloadStatus.Enqueued or DownloadStatus.Started;

    /// <summary>
    /// 取消下载任务
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        if (_isDisposed)
            return;

        _cancellationTokenSource.Cancel();
    }

    /// <summary>
    /// 检查是否可以在文件资源管理器中显示文件
    /// </summary>
    private bool CanShowFile() =>
        Status == DownloadStatus.Completed
        // 此功能目前仅在Windows上可用
        && OperatingSystem.IsWindows();

    /// <summary>
    /// 在文件资源管理器中显示下载的文件
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanShowFile))]
    private async Task ShowFileAsync()
    {
        if (string.IsNullOrWhiteSpace(FilePath))
            return;

        try
        {
            // 在Windows资源管理器中导航到文件并选中它
            ProcessEx.Start("explorer", ["/select,", FilePath]);
        }
        catch (Exception ex)
        {
            await _dialogManager.ShowDialogAsync(
                _viewModelManager.CreateMessageBoxViewModel("Error", ex.Message)
            );
        }
    }

    /// <summary>
    /// 检查是否可以打开下载的文件
    /// </summary>
    private bool CanOpenFile() => Status == DownloadStatus.Completed;

    /// <summary>
    /// 使用系统默认程序打开下载的文件
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanOpenFile))]
    private async Task OpenFileAsync()
    {
        if (string.IsNullOrWhiteSpace(FilePath))
            return;

        try
        {
            ProcessEx.StartShellExecute(FilePath);
        }
        catch (Exception ex)
        {
            await _dialogManager.ShowDialogAsync(
                _viewModelManager.CreateMessageBoxViewModel("Error", ex.Message)
            );
        }
    }

    /// <summary>
    /// 复制错误信息到剪贴板
    /// </summary>
    [RelayCommand]
    private async Task CopyErrorMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(ErrorMessage))
            return;

        if (Application.Current?.ApplicationLifetime?.TryGetTopLevel()?.Clipboard is { } clipboard)
            await clipboard.SetTextAsync(ErrorMessage);
    }

    /// <summary>
    /// 释放资源并执行清理操作
    /// </summary>
    /// <param name="disposing">是否正在释放托管资源</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _eventRoot.Dispose();
            _cancellationTokenSource.Dispose();

            _isDisposed = true;
        }

        base.Dispose(disposing);
    }
}
