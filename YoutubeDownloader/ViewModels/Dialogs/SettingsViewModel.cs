using System;
using System.Collections.Generic;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Extensions;

namespace YoutubeDownloader.ViewModels.Dialogs;

/// <summary>
/// 设置对话框的视图模型，用于管理和保存应用程序的各种设置
/// </summary>
public class SettingsViewModel : DialogViewModelBase
{
    private readonly SettingsService _settingsService;

    /// <summary>
    /// 事件订阅收集器，用于管理和清理事件订阅
    /// </summary>
    private readonly DisposableCollector _eventRoot = new();

    /// <summary>
    /// 构造函数，初始化设置视图模型
    /// </summary>
    /// <param name="settingsService">设置服务，用于存储和读取设置</param>
    public SettingsViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;

        // 监听所有设置属性的变化
        _eventRoot.Add(_settingsService.WatchAllProperties(OnAllPropertiesChanged));
    }

    /// <summary>
    /// 可用的主题列表
    /// </summary>
    public IReadOnlyList<ThemeVariant> AvailableThemes { get; } = Enum.GetValues<ThemeVariant>();

    /// <summary>
    /// 当前选择的主题
    /// </summary>
    public ThemeVariant Theme
    {
        get => _settingsService.Theme;
        set => _settingsService.Theme = value;
    }

    /// <summary>
    /// 是否启用自动更新
    /// </summary>
    public bool IsAutoUpdateEnabled
    {
        get => _settingsService.IsAutoUpdateEnabled;
        set => _settingsService.IsAutoUpdateEnabled = value;
    }

    /// <summary>
    /// 是否持久化保存认证信息
    /// </summary>
    public bool IsAuthPersisted
    {
        get => _settingsService.IsAuthPersisted;
        set => _settingsService.IsAuthPersisted = value;
    }

    /// <summary>
    /// 是否注入特定语言的音频流
    /// </summary>
    public bool ShouldInjectLanguageSpecificAudioStreams
    {
        get => _settingsService.ShouldInjectLanguageSpecificAudioStreams;
        set => _settingsService.ShouldInjectLanguageSpecificAudioStreams = value;
    }

    /// <summary>
    /// 是否注入字幕
    /// </summary>
    public bool ShouldInjectSubtitles
    {
        get => _settingsService.ShouldInjectSubtitles;
        set => _settingsService.ShouldInjectSubtitles = value;
    }

    /// <summary>
    /// 是否注入媒体标签
    /// </summary>
    public bool ShouldInjectTags
    {
        get => _settingsService.ShouldInjectTags;
        set => _settingsService.ShouldInjectTags = value;
    }

    /// <summary>
    /// 是否跳过已存在的文件
    /// </summary>
    public bool ShouldSkipExistingFiles
    {
        get => _settingsService.ShouldSkipExistingFiles;
        set => _settingsService.ShouldSkipExistingFiles = value;
    }

    /// <summary>
    /// 文件命名模板
    /// </summary>
    public string FileNameTemplate
    {
        get => _settingsService.FileNameTemplate;
        set => _settingsService.FileNameTemplate = value;
    }

    /// <summary>
    /// 并行下载的最大数量，限制在1-10之间
    /// </summary>
    public int ParallelLimit
    {
        get => _settingsService.ParallelLimit;
        set => _settingsService.ParallelLimit = Math.Clamp(value, 1, 10);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing">是否正在释放托管资源</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _eventRoot.Dispose();
        }

        base.Dispose(disposing);
    }
}
