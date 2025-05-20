using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Core.Utils.Extensions;
using YoutubeDownloader.ViewModels;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Framework;

/// <summary>
/// 视图模型管理器，负责创建和初始化应用程序中的各种视图模型
/// </summary>
/// <param name="services">服务提供者，用于获取已注册的视图模型实例</param>
public class ViewModelManager(IServiceProvider services)
{
    /// <summary>
    /// 创建主视图模型
    /// </summary>
    /// <returns>初始化后的主视图模型实例</returns>
    public MainViewModel CreateMainViewModel() => services.GetRequiredService<MainViewModel>();

    /// <summary>
    /// 创建仪表板视图模型
    /// </summary>
    /// <returns>初始化后的仪表板视图模型实例</returns>
    public DashboardViewModel CreateDashboardViewModel() =>
        services.GetRequiredService<DashboardViewModel>();

    /// <summary>
    /// 创建认证设置视图模型
    /// </summary>
    /// <returns>初始化后的认证设置视图模型实例</returns>
    public AuthSetupViewModel CreateAuthSetupViewModel() =>
        services.GetRequiredService<AuthSetupViewModel>();

    /// <summary>
    /// 创建下载视图模型（使用指定的下载选项）
    /// </summary>
    /// <param name="video">要下载的视频</param>
    /// <param name="downloadOption">视频下载选项</param>
    /// <param name="filePath">文件保存路径</param>
    /// <returns>初始化后的下载视图模型实例</returns>
    public DownloadViewModel CreateDownloadViewModel(
        IVideo video,
        VideoDownloadOption downloadOption,
        string filePath
    )
    {
        var viewModel = services.GetRequiredService<DownloadViewModel>();

        viewModel.Video = video;
        viewModel.DownloadOption = downloadOption;
        viewModel.FilePath = filePath;

        return viewModel;
    }

    /// <summary>
    /// 创建下载视图模型（使用下载偏好设置）
    /// </summary>
    /// <param name="video">要下载的视频</param>
    /// <param name="downloadPreference">视频下载偏好设置</param>
    /// <param name="filePath">文件保存路径</param>
    /// <returns>初始化后的下载视图模型实例</returns>
    public DownloadViewModel CreateDownloadViewModel(
        IVideo video,
        VideoDownloadPreference downloadPreference,
        string filePath
    )
    {
        var viewModel = services.GetRequiredService<DownloadViewModel>();

        viewModel.Video = video;
        viewModel.DownloadPreference = downloadPreference;
        viewModel.FilePath = filePath;

        return viewModel;
    }

    /// <summary>
    /// 创建多视频下载设置视图模型
    /// </summary>
    /// <param name="title">对话框标题</param>
    /// <param name="availableVideos">可供下载的视频列表</param>
    /// <param name="preselectVideos">是否预先选中所有视频，默认为true</param>
    /// <returns>初始化后的多视频下载设置视图模型实例</returns>
    public DownloadMultipleSetupViewModel CreateDownloadMultipleSetupViewModel(
        string title,
        IReadOnlyList<IVideo> availableVideos,
        bool preselectVideos = true
    )
    {
        var viewModel = services.GetRequiredService<DownloadMultipleSetupViewModel>();

        viewModel.Title = title;
        viewModel.AvailableVideos = availableVideos;

        if (preselectVideos)
            viewModel.SelectedVideos.AddRange(availableVideos);

        return viewModel;
    }

    /// <summary>
    /// 创建单视频下载设置视图模型
    /// </summary>
    /// <param name="video">要下载的视频</param>
    /// <param name="availableDownloadOptions">可用的下载选项列表</param>
    /// <returns>初始化后的单视频下载设置视图模型实例</returns>
    public DownloadSingleSetupViewModel CreateDownloadSingleSetupViewModel(
        IVideo video,
        IReadOnlyList<VideoDownloadOption> availableDownloadOptions
    )
    {
        var viewModel = services.GetRequiredService<DownloadSingleSetupViewModel>();

        viewModel.Video = video;
        viewModel.AvailableDownloadOptions = availableDownloadOptions;

        return viewModel;
    }

    /// <summary>
    /// 创建消息框视图模型（带确认和取消按钮）
    /// </summary>
    /// <param name="title">消息框标题</param>
    /// <param name="message">消息内容</param>
    /// <param name="okButtonText">确认按钮文本</param>
    /// <param name="cancelButtonText">取消按钮文本</param>
    /// <returns>初始化后的消息框视图模型实例</returns>
    public MessageBoxViewModel CreateMessageBoxViewModel(
        string title,
        string message,
        string? okButtonText,
        string? cancelButtonText
    )
    {
        var viewModel = services.GetRequiredService<MessageBoxViewModel>();

        viewModel.Title = title;
        viewModel.Message = message;
        viewModel.DefaultButtonText = okButtonText;
        viewModel.CancelButtonText = cancelButtonText;

        return viewModel;
    }

    /// <summary>
    /// 创建简单消息框视图模型（只有关闭按钮）
    /// </summary>
    /// <param name="title">消息框标题</param>
    /// <param name="message">消息内容</param>
    /// <returns>初始化后的消息框视图模型实例</returns>
    public MessageBoxViewModel CreateMessageBoxViewModel(string title, string message) =>
        CreateMessageBoxViewModel(title, message, "CLOSE", null);

    /// <summary>
    /// 创建设置视图模型
    /// </summary>
    /// <returns>初始化后的设置视图模型实例</returns>
    public SettingsViewModel CreateSettingsViewModel() =>
        services.GetRequiredService<SettingsViewModel>();
}
