using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Extensions;
using YoutubeDownloader.ViewModels.Components;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Dialogs;

/// <summary>
/// 单个视频下载设置对话框的视图模型，用于配置和启动单个视频的下载
/// </summary>
public partial class DownloadSingleSetupViewModel(
    ViewModelManager viewModelManager,
    DialogManager dialogManager,
    SettingsService settingsService
) : DialogViewModelBase<DownloadViewModel>
{
    /// <summary>
    /// 要下载的视频信息
    /// </summary>
    [ObservableProperty]
    public partial IVideo? Video { get; set; }

    /// <summary>
    /// 可用的下载选项列表，包含不同的格式和质量
    /// </summary>
    [ObservableProperty]
    public partial IReadOnlyList<VideoDownloadOption>? AvailableDownloadOptions { get; set; }

    /// <summary>
    /// 用户选中的下载选项
    /// </summary>
    [ObservableProperty]
    public partial VideoDownloadOption? SelectedDownloadOption { get; set; }

    /// <summary>
    /// 初始化视图模型，设置默认下载选项
    /// </summary>
    [RelayCommand]
    private void Initialize()
    {
        // 尝试选择与上次使用的容器格式相同的下载选项
        SelectedDownloadOption = AvailableDownloadOptions?.FirstOrDefault(o =>
            o.Container == settingsService.LastContainer
        );
    }

    /// <summary>
    /// 复制视频标题到剪贴板
    /// </summary>
    [RelayCommand]
    private async Task CopyTitleAsync()
    {
        if (Application.Current?.ApplicationLifetime?.TryGetTopLevel()?.Clipboard is { } clipboard)
            await clipboard.SetTextAsync(Video?.Title);
    }

    /// <summary>
    /// 确认下载视频
    /// </summary>
    [RelayCommand]
    private async Task ConfirmAsync()
    {
        if (Video is null || SelectedDownloadOption is null)
            return;

        var container = SelectedDownloadOption.Container;

        // 提示用户选择保存文件路径
        var filePath = await dialogManager.PromptSaveFilePathAsync(
            [
                new FilePickerFileType($"{container.Name} file")
                {
                    Patterns = [$"*.{container.Name}"],
                },
            ],
            FileNameTemplate.Apply(settingsService.FileNameTemplate, Video, container)
        );

        if (string.IsNullOrWhiteSpace(filePath))
            return;

        // 下载不会立即开始，所以先锁定文件路径以避免冲突
        DirectoryEx.CreateDirectoryForFile(filePath);
        await File.WriteAllBytesAsync(filePath, []);

        // 保存用户选择的容器格式
        settingsService.LastContainer = container;

        // 关闭对话框并返回下载任务
        Close(viewModelManager.CreateDownloadViewModel(Video, SelectedDownloadOption, filePath));
    }
}
