using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Extensions;
using YoutubeDownloader.ViewModels.Components;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.ViewModels.Dialogs;

/// <summary>
/// 多视频下载设置对话框的视图模型，用于配置和启动多个视频的下载
/// </summary>
public partial class DownloadMultipleSetupViewModel(
    ViewModelManager viewModelManager,
    DialogManager dialogManager,
    SettingsService settingsService
) : DialogViewModelBase<IReadOnlyList<DownloadViewModel>>
{
    /// <summary>
    /// 视频集合的标题
    /// </summary>
    [ObservableProperty]
    public partial string? Title { get; set; }

    /// <summary>
    /// 可供选择的视频列表
    /// </summary>
    [ObservableProperty]
    public partial IReadOnlyList<IVideo>? AvailableVideos { get; set; }

    /// <summary>
    /// 选中的容器格式（如MP4、WebM等）
    /// </summary>
    [ObservableProperty]
    public partial Container SelectedContainer { get; set; } = Container.Mp4;

    /// <summary>
    /// 选中的视频质量偏好
    /// </summary>
    [ObservableProperty]
    public partial VideoQualityPreference SelectedVideoQualityPreference { get; set; } =
        VideoQualityPreference.Highest;

    /// <summary>
    /// 用户选中的要下载的视频集合
    /// </summary>
    public ObservableCollection<IVideo> SelectedVideos { get; } = [];

    /// <summary>
    /// 可用的容器格式列表
    /// </summary>
    public IReadOnlyList<Container> AvailableContainers { get; } =
        [Container.Mp4, Container.WebM, Container.Mp3, new("ogg")];

    /// <summary>
    /// 可用的视频质量偏好列表，按从高到低排序
    /// </summary>
    public IReadOnlyList<VideoQualityPreference> AvailableVideoQualityPreferences { get; } =
        // Without .AsEnumerable(), the below line throws a compile-time error starting with .NET SDK v9.0.200
        Enum.GetValues<VideoQualityPreference>().AsEnumerable().Reverse().ToArray();

    /// <summary>
    /// 初始化视图模型，设置默认值并绑定事件
    /// </summary>
    [RelayCommand]
    private void Initialize()
    {
        // 使用上次的设置作为默认值
        SelectedContainer = settingsService.LastContainer;
        SelectedVideoQualityPreference = settingsService.LastVideoQualityPreference;
        // 监听选中视频集合变化，更新确认按钮状态
        SelectedVideos.CollectionChanged += (_, _) => ConfirmCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// 复制标题到剪贴板
    /// </summary>
    [RelayCommand]
    private async Task CopyTitleAsync()
    {
        if (Application.Current?.ApplicationLifetime?.TryGetTopLevel()?.Clipboard is { } clipboard)
            await clipboard.SetTextAsync(Title);
    }

    /// <summary>
    /// 检查是否可以确认下载（至少选择了一个视频）
    /// </summary>
    private bool CanConfirm() => SelectedVideos.Any();

    /// <summary>
    /// 确认下载选中的视频
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private async Task ConfirmAsync()
    {
        // 提示用户选择保存目录
        var dirPath = await dialogManager.PromptDirectoryPathAsync();
        if (string.IsNullOrWhiteSpace(dirPath))
            return;

        var downloads = new List<DownloadViewModel>();
        for (var i = 0; i < SelectedVideos.Count; i++)
        {
            var video = SelectedVideos[i];

            // 根据模板生成文件路径
            var baseFilePath = Path.Combine(
                dirPath,
                FileNameTemplate.Apply(
                    settingsService.FileNameTemplate,
                    video,
                    SelectedContainer,
                    (i + 1).ToString().PadLeft(SelectedVideos.Count.ToString().Length, '0')
                )
            );

            // 如果设置了跳过已存在的文件且文件已存在，则跳过
            if (settingsService.ShouldSkipExistingFiles && File.Exists(baseFilePath))
                continue;

            // 确保文件路径唯一
            var filePath = PathEx.EnsureUniquePath(baseFilePath);

            // 下载不会立即开始，所以先锁定文件路径以避免冲突
            DirectoryEx.CreateDirectoryForFile(filePath);
            await File.WriteAllBytesAsync(filePath, []);

            // 创建下载视图模型并添加到列表
            downloads.Add(
                viewModelManager.CreateDownloadViewModel(
                    video,
                    new VideoDownloadPreference(SelectedContainer, SelectedVideoQualityPreference),
                    filePath
                )
            );
        }

        // 保存用户选择的设置
        settingsService.LastContainer = SelectedContainer;
        settingsService.LastVideoQualityPreference = SelectedVideoQualityPreference;

        // 关闭对话框并返回下载任务列表
        Close(downloads);
    }
}
