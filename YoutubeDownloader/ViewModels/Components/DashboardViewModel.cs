using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gress;
using Gress.Completable;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Core.Resolving;
using YoutubeDownloader.Core.Tagging;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Extensions;
using YoutubeExplode.Exceptions;

namespace YoutubeDownloader.ViewModels.Components;

/// <summary>
/// 仪表板视图模型，负责处理YouTube视频的查询、解析和下载
/// </summary>
public partial class DashboardViewModel : ViewModelBase
{
    private readonly ViewModelManager _viewModelManager;
    private readonly SnackbarManager _snackbarManager;
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;

    private readonly DisposableCollector _eventRoot = new();
    private readonly ResizableSemaphore _downloadSemaphore = new();
    private readonly AutoResetProgressMuxer _progressMuxer;

    /// <summary>
    /// 初始化仪表板视图模型
    /// </summary>
    public DashboardViewModel(
        ViewModelManager viewModelManager,
        SnackbarManager snackbarManager,
        DialogManager dialogManager,
        SettingsService settingsService
    )
    {
        _viewModelManager = viewModelManager;
        _snackbarManager = snackbarManager;
        _dialogManager = dialogManager;
        _settingsService = settingsService;

        // 创建自动重置的进度多路复用器
        _progressMuxer = Progress.CreateMuxer().WithAutoReset();

        // 监听并应用并行下载限制设置的变化
        _eventRoot.Add(
            _settingsService.WatchProperty(
                o => o.ParallelLimit,
                () => _downloadSemaphore.MaxCount = _settingsService.ParallelLimit,
                true
            )
        );

        // 监听进度变化以更新进度指示器状态
        _eventRoot.Add(
            Progress.WatchProperty(
                o => o.Current,
                () => OnPropertyChanged(nameof(IsProgressIndeterminate))
            )
        );
    }

    /// <summary>
    /// 指示是否正在处理操作
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProgressIndeterminate))]
    [NotifyCanExecuteChangedFor(nameof(ProcessQueryCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowAuthSetupCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowSettingsCommand))]
    public partial bool IsBusy { get; set; }

    /// <summary>
    /// 进度容器，用于跟踪和显示操作进度
    /// </summary>
    public ProgressContainer<Percentage> Progress { get; } = new();

    /// <summary>
    /// 指示进度是否为不确定状态
    /// </summary>
    public bool IsProgressIndeterminate => IsBusy && Progress.Current.Fraction is <= 0 or >= 1;

    /// <summary>
    /// 用户输入的查询字符串
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ProcessQueryCommand))]
    public partial string? Query { get; set; }

    /// <summary>
    /// 下载任务集合
    /// </summary>
    public ObservableCollection<DownloadViewModel> Downloads { get; } = [];

    /// <summary>
    /// 检查是否可以显示认证设置
    /// </summary>
    private bool CanShowAuthSetup() => !IsBusy;

    /// <summary>
    /// 显示认证设置对话框
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanShowAuthSetup))]
    private async Task ShowAuthSetupAsync() =>
        await _dialogManager.ShowDialogAsync(_viewModelManager.CreateAuthSetupViewModel());

    /// <summary>
    /// 检查是否可以显示设置
    /// </summary>
    private bool CanShowSettings() => !IsBusy;

    /// <summary>
    /// 显示设置对话框
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanShowSettings))]
    private async Task ShowSettingsAsync() =>
        await _dialogManager.ShowDialogAsync(_viewModelManager.CreateSettingsViewModel());

    /// <summary>
    /// 将下载任务添加到队列并开始下载
    /// </summary>
    /// <param name="download">下载视图模型</param>
    /// <param name="position">在队列中的位置，默认为0（队列开头）</param>
    private async void EnqueueDownload(DownloadViewModel download, int position = 0)
    {
        Downloads.Insert(position, download);
        var progress = _progressMuxer.CreateInput();

        try
        {
            var downloader = new VideoDownloader(_settingsService.LastAuthCookies);
            var tagInjector = new MediaTagInjector();

            // 获取下载信号量，限制并行下载数量
            using var access = await _downloadSemaphore.AcquireAsync(download.CancellationToken);

            download.Status = DownloadStatus.Started;

            // 获取最佳下载选项（如果尚未指定）
            var downloadOption =
                download.DownloadOption
                ?? await downloader.GetBestDownloadOptionAsync(
                    download.Video!.Id,
                    download.DownloadPreference!,
                    _settingsService.ShouldInjectLanguageSpecificAudioStreams,
                    download.CancellationToken
                );

            // 执行视频下载
            await downloader.DownloadVideoAsync(
                download.FilePath!,
                download.Video!,
                downloadOption,
                _settingsService.ShouldInjectSubtitles,
                download.Progress.Merge(progress),
                download.CancellationToken
            );

            // 如果设置了注入标签，则注入媒体标签
            if (_settingsService.ShouldInjectTags)
            {
                try
                {
                    await tagInjector.InjectTagsAsync(
                        download.FilePath!,
                        download.Video!,
                        download.CancellationToken
                    );
                }
                catch
                {
                    // 媒体标签注入不是关键操作，失败可以忽略
                }
            }

            download.Status = DownloadStatus.Completed;
        }
        catch (Exception ex)
        {
            try
            {
                // 删除未完成的下载文件
                if (!string.IsNullOrWhiteSpace(download.FilePath))
                    File.Delete(download.FilePath);
            }
            catch
            {
                // 忽略删除文件时的错误
            }

            // 根据异常类型设置下载状态
            download.Status =
                ex is OperationCanceledException ? DownloadStatus.Canceled : DownloadStatus.Failed;

            // YouTube相关错误显示简短消息，其他错误显示完整堆栈
            download.ErrorMessage = ex is YoutubeExplodeException ? ex.Message : ex.ToString();
        }
        finally
        {
            // 报告进度完成并释放资源
            progress.ReportCompletion();
            download.Dispose();
        }
    }

    /// <summary>
    /// 检查是否可以处理查询
    /// </summary>
    private bool CanProcessQuery() => !IsBusy && !string.IsNullOrWhiteSpace(Query);

    /// <summary>
    /// 处理用户输入的查询，解析并准备下载
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanProcessQuery))]
    private async Task ProcessQueryAsync()
    {
        if (string.IsNullOrWhiteSpace(Query))
            return;

        IsBusy = true;

        // 小权重，以不影响现有下载操作
        var progress = _progressMuxer.CreateInput(0.01);

        try
        {
            var resolver = new QueryResolver(_settingsService.LastAuthCookies);
            var downloader = new VideoDownloader(_settingsService.LastAuthCookies);

            // 按换行符分割查询
            var queries = Query.Split(
                '\n',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );

            // 处理单个查询
            var queryResults = new List<QueryResult>();
            foreach (var (i, query) in queries.Index())
            {
                try
                {
                    queryResults.Add(await resolver.ResolveAsync(query));
                }
                // 如果不是列表中唯一的查询，不中断处理过程
                // 通过异步通知而不是同步对话框报告错误
                catch (YoutubeExplodeException ex)
                    when (ex is VideoUnavailableException or PlaylistUnavailableException
                        && queries.Length > 1
                    )
                {
                    _snackbarManager.Notify(ex.Message);
                }

                progress.Report(Percentage.FromFraction((i + 1.0) / queries.Length));
            }

            // 聚合结果
            var queryResult = QueryResult.Aggregate(queryResults);

            // 单个视频结果
            if (queryResult.Videos.Count == 1)
            {
                var video = queryResult.Videos.Single();

                var downloadOptions = await downloader.GetDownloadOptionsAsync(
                    video.Id,
                    _settingsService.ShouldInjectLanguageSpecificAudioStreams
                );

                var download = await _dialogManager.ShowDialogAsync(
                    _viewModelManager.CreateDownloadSingleSetupViewModel(video, downloadOptions)
                );

                if (download is null)
                    return;

                EnqueueDownload(download);

                Query = "";
            }
            // 多个视频
            else if (queryResult.Videos.Count > 1)
            {
                var downloads = await _dialogManager.ShowDialogAsync(
                    _viewModelManager.CreateDownloadMultipleSetupViewModel(
                        queryResult.Title,
                        queryResult.Videos,
                        // 如果视频来自单个查询而非搜索，则预选视频
                        queryResult.Kind
                            is not QueryResultKind.Search
                                and not QueryResultKind.Aggregate
                    )
                );

                if (downloads is null)
                    return;

                foreach (var download in downloads)
                    EnqueueDownload(download);

                Query = "";
            }
            // 未找到视频
            else
            {
                await _dialogManager.ShowDialogAsync(
                    _viewModelManager.CreateMessageBoxViewModel(
                        "Nothing found",
                        "Couldn't find any videos based on the query or URL you provided"
                    )
                );
            }
        }
        catch (Exception ex)
        {
            await _dialogManager.ShowDialogAsync(
                _viewModelManager.CreateMessageBoxViewModel(
                    "Error",
                    // YouTube相关错误显示简短消息，其他错误显示完整堆栈
                    ex is YoutubeExplodeException
                        ? ex.Message
                        : ex.ToString()
                )
            );
        }
        finally
        {
            progress.ReportCompletion();
            IsBusy = false;
        }
    }

    /// <summary>
    /// 从下载队列中移除下载任务
    /// </summary>
    private void RemoveDownload(DownloadViewModel download)
    {
        Downloads.Remove(download);
        download.CancelCommand.Execute(null);
        download.Dispose();
    }

    /// <summary>
    /// 移除所有已成功完成的下载任务
    /// </summary>
    [RelayCommand]
    private void RemoveSuccessfulDownloads()
    {
        foreach (var download in Downloads.ToArray())
        {
            if (download.Status == DownloadStatus.Completed)
                RemoveDownload(download);
        }
    }

    /// <summary>
    /// 移除所有非活动状态的下载任务（已完成、失败或取消）
    /// </summary>
    [RelayCommand]
    private void RemoveInactiveDownloads()
    {
        foreach (var download in Downloads.ToArray())
        {
            if (
                download.Status
                is DownloadStatus.Completed
                    or DownloadStatus.Failed
                    or DownloadStatus.Canceled
            )
                RemoveDownload(download);
        }
    }

    /// <summary>
    /// 重新启动指定的下载任务
    /// </summary>
    [RelayCommand]
    private void RestartDownload(DownloadViewModel download)
    {
        var position = Math.Max(0, Downloads.IndexOf(download));
        RemoveDownload(download);

        // 根据下载选项或下载偏好创建新的下载任务
        var newDownload = download.DownloadOption is not null
            ? _viewModelManager.CreateDownloadViewModel(
                download.Video!,
                download.DownloadOption,
                download.FilePath!
            )
            : _viewModelManager.CreateDownloadViewModel(
                download.Video!,
                download.DownloadPreference!,
                download.FilePath!
            );

        EnqueueDownload(newDownload, position);
    }

    /// <summary>
    /// 重新启动所有失败的下载任务
    /// </summary>
    [RelayCommand]
    private void RestartFailedDownloads()
    {
        foreach (var download in Downloads.ToArray())
        {
            if (download.Status == DownloadStatus.Failed)
                RestartDownload(download);
        }
    }

    /// <summary>
    /// 取消所有下载任务
    /// </summary>
    [RelayCommand]
    private void CancelAllDownloads()
    {
        foreach (var download in Downloads)
            download.CancelCommand.Execute(null);
    }

    /// <summary>
    /// 释放资源并执行清理操作
    /// </summary>
    /// <param name="disposing">是否正在释放托管资源</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            CancelAllDownloads();

            _eventRoot.Dispose();
            _downloadSemaphore.Dispose();
        }

        base.Dispose(disposing);
    }
}
