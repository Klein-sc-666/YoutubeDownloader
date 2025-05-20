using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using AvaloniaWebView;
using Material.Styles.Themes;
using Microsoft.Extensions.DependencyInjection;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Extensions;
using YoutubeDownloader.ViewModels;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.Views;

namespace YoutubeDownloader;

/// <summary>
/// 应用程序主类，负责初始化应用程序、管理依赖注入和主题设置
/// 实现IDisposable接口以确保资源正确释放
/// </summary>
public class App : Application, IDisposable
{
    /// <summary>
    /// 事件订阅收集器，用于管理和清理事件订阅
    /// </summary>
    private readonly DisposableCollector _eventRoot = new();

    /// <summary>
    /// 依赖注入服务提供者
    /// </summary>
    private readonly ServiceProvider _services;

    /// <summary>
    /// 应用程序设置服务
    /// </summary>
    private readonly SettingsService _settingsService;

    /// <summary>
    /// 主视图模型
    /// </summary>
    private readonly MainViewModel _mainViewModel;

    /// <summary>
    /// 构造函数，初始化依赖注入和应用程序服务
    /// </summary>
    public App()
    {
        var services = new ServiceCollection();

        // Framework
        services.AddSingleton<DialogManager>();
        services.AddSingleton<SnackbarManager>();
        services.AddSingleton<ViewManager>();
        services.AddSingleton<ViewModelManager>();

        // Services
        services.AddSingleton<SettingsService>();
        services.AddSingleton<UpdateService>();

        // View models
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DownloadViewModel>();
        services.AddTransient<AuthSetupViewModel>();
        services.AddTransient<DownloadMultipleSetupViewModel>();
        services.AddTransient<DownloadSingleSetupViewModel>();
        services.AddTransient<MessageBoxViewModel>();
        services.AddTransient<SettingsViewModel>();

        _services = services.BuildServiceProvider(true);
        _settingsService = _services.GetRequiredService<SettingsService>();
        _mainViewModel = _services.GetRequiredService<ViewModelManager>().CreateMainViewModel();

        // Re-initialize the theme when the user changes it
        _eventRoot.Add(
            _settingsService.WatchProperty(
                o => o.Theme,
                () =>
                {
                    RequestedThemeVariant = _settingsService.Theme switch
                    {
                        ThemeVariant.Light => Avalonia.Styling.ThemeVariant.Light,
                        ThemeVariant.Dark => Avalonia.Styling.ThemeVariant.Dark,
                        _ => Avalonia.Styling.ThemeVariant.Default,
                    };

                    InitializeTheme();
                }
            )
        );
    }

    /// <summary>
    /// 初始化应用程序
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();

        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// 注册应用程序服务
    /// </summary>
    public override void RegisterServices()
    {
        base.RegisterServices();

        AvaloniaWebViewBuilder.Initialize(config => config.IsInPrivateModeEnabled = true);
    }

    /// <summary>
    /// 初始化应用程序主题
    /// </summary>
    private void InitializeTheme()
    {
        var actualTheme = RequestedThemeVariant?.Key switch
        {
            "Light" => PlatformThemeVariant.Light,
            "Dark" => PlatformThemeVariant.Dark,
            _ => PlatformSettings?.GetColorValues().ThemeVariant ?? PlatformThemeVariant.Light,
        };

        this.LocateMaterialTheme<MaterialThemeBase>().CurrentTheme =
            actualTheme == PlatformThemeVariant.Light
                ? Theme.Create(Theme.Light, Color.Parse("#343838"), Color.Parse("#F9A825"))
                : Theme.Create(Theme.Dark, Color.Parse("#E8E8E8"), Color.Parse("#F9A825"));
    }

    /// <summary>
    /// 框架初始化完成后的回调方法
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainView { DataContext = _mainViewModel };

        base.OnFrameworkInitializationCompleted();

        // Set up custom theme colors
        InitializeTheme();

        // Load settings
        _settingsService.Load();
    }

    /// <summary>
    /// 系统主题变更事件处理方法
    /// </summary>
    private void Application_OnActualThemeVariantChanged(object? sender, EventArgs args) =>
        // Re-initialize the theme when the system theme changes
        InitializeTheme();

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _eventRoot.Dispose();
        _services.Dispose();
    }
}
