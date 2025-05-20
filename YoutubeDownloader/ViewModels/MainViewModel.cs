using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.Input;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Extensions;
using YoutubeDownloader.ViewModels.Components;

namespace YoutubeDownloader.ViewModels;

/// <summary>
/// 主视图模型，负责应用程序的主要功能和初始化流程
/// </summary>
public partial class MainViewModel(
    ViewModelManager viewModelManager,
    DialogManager dialogManager,
    SnackbarManager snackbarManager,
    SettingsService settingsService,
    UpdateService updateService
) : ViewModelBase
{
    /// <summary>
    /// 获取应用程序标题，包含程序名称和版本号
    /// </summary>
    public string Title { get; } = $"{Program.Name} v{Program.VersionString}";

    /// <summary>
    /// 获取仪表板视图模型，用于处理下载和查询功能
    /// </summary>
    public DashboardViewModel Dashboard { get; } = viewModelManager.CreateDashboardViewModel();

    /// <summary>
    /// 释放资源并执行清理操作
    /// </summary>
    /// <param name="disposing">是否正在释放托管资源</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // 保存设置
            settingsService.Save();

            // 完成待处理的更新
            updateService.FinalizeUpdate(false);
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// 显示乌克兰支持信息对话框
    /// </summary>
    private async Task ShowUkraineSupportMessageAsync()
    {
        if (!settingsService.IsUkraineSupportMessageEnabled)
            return;

        var dialog = viewModelManager.CreateMessageBoxViewModel(
            "Thank you for supporting Ukraine!",
            """
            As Russia wages a genocidal war against my country, I'm grateful to everyone who continues to stand with Ukraine in our fight for freedom.

            Click LEARN MORE to find ways that you can help.
            """,
            "LEARN MORE",
            "CLOSE"
        );

        // 禁用此消息在未来显示
        settingsService.IsUkraineSupportMessageEnabled = false;
        settingsService.Save();

        if (await dialogManager.ShowDialogAsync(dialog) == true)
            ProcessEx.StartShellExecute("https://tyrrrz.me/ukraine?source=youtubedownloader");
    }

    /// <summary>
    /// 显示开发版本警告对话框
    /// </summary>
    private async Task ShowDevelopmentBuildMessageAsync()
    {
        if (!Program.IsDevelopmentBuild)
            return;

        // 如果正在调试，用户可能是开发人员
        if (Debugger.IsAttached)
            return;

        var dialog = viewModelManager.CreateMessageBoxViewModel(
            "Unstable build warning",
            $"""
            You're using a development build of {Program.Name}. These builds are not thoroughly tested and may contain bugs.

            Auto-updates are disabled for development builds.

            Click SEE RELEASES if you want to download a stable release instead.
            """,
            "SEE RELEASES",
            "CLOSE"
        );

        if (await dialogManager.ShowDialogAsync(dialog) == true)
            ProcessEx.StartShellExecute(Program.ProjectReleasesUrl);
    }

    /// <summary>
    /// 检查FFmpeg是否可用，如果不可用则显示警告对话框
    /// </summary>
    private async Task ShowFFmpegMessageAsync()
    {
        if (FFmpeg.IsAvailable())
            return;

        var dialog = viewModelManager.CreateMessageBoxViewModel(
            "FFmpeg is missing",
            $"""
            FFmpeg is required for {Program.Name} to work. Please download it and make it available in the application directory or on the system PATH.

            Alternatively, you can also download a version of {Program.Name} that has FFmpeg bundled with it. Look for release assets that are NOT marked as *.Bare.

            Click DOWNLOAD to go to the FFmpeg download page.
            """,
            "DOWNLOAD",
            "CLOSE"
        );

        if (await dialogManager.ShowDialogAsync(dialog) == true)
            ProcessEx.StartShellExecute("https://ffmpeg.org/download.html");

        if (Application.Current?.ApplicationLifetime?.TryShutdown(3) != true)
            Environment.Exit(3);
    }

    /// <summary>
    /// 检查应用程序更新并下载
    /// </summary>
    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var updateVersion = await updateService.CheckForUpdatesAsync();
            if (updateVersion is null)
                return;

            snackbarManager.Notify($"Downloading update to {Program.Name} v{updateVersion}...");
            await updateService.PrepareUpdateAsync(updateVersion);

            snackbarManager.Notify(
                "Update has been downloaded and will be installed when you exit",
                "INSTALL NOW",
                () =>
                {
                    updateService.FinalizeUpdate(true);

                    if (Application.Current?.ApplicationLifetime?.TryShutdown(2) != true)
                        Environment.Exit(2);
                }
            );
        }
        catch
        {
            // 更新失败不应导致应用程序崩溃
            snackbarManager.Notify("Failed to perform application update");
        }
    }

    /// <summary>
    /// 初始化应用程序，显示必要的对话框并检查更新
    /// </summary>
    [RelayCommand]
    private async Task InitializeAsync()
    {
        await ShowUkraineSupportMessageAsync();
        await ShowDevelopmentBuildMessageAsync();
        await ShowFFmpegMessageAsync();
        await CheckForUpdatesAsync();
    }
}
