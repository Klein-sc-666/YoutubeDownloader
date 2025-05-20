using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Extensions;

namespace YoutubeDownloader.ViewModels.Dialogs;

/// <summary>
/// 认证设置对话框的视图模型，用于管理YouTube账号的登录状态和认证Cookie
/// </summary>
public class AuthSetupViewModel : DialogViewModelBase
{
    private readonly SettingsService _settingsService;

    /// <summary>
    /// 事件订阅收集器，用于管理和清理事件订阅
    /// </summary>
    private readonly DisposableCollector _eventRoot = new();

    /// <summary>
    /// 获取或设置认证Cookie集合
    /// </summary>
    public IReadOnlyList<Cookie>? Cookies
    {
        get => _settingsService.LastAuthCookies;
        set => _settingsService.LastAuthCookies = value;
    }

    /// <summary>
    /// 判断用户是否已认证
    /// 条件：1. 存在Cookie；2. 所有以"__SECURE"开头的Cookie都未过期
    /// </summary>
    public bool IsAuthenticated =>
        Cookies?.Any() == true
        &&
        // None of the '__SECURE' cookies should be expired
        Cookies
            .Where(c => c.Name.StartsWith("__SECURE", StringComparison.OrdinalIgnoreCase))
            .All(c => !c.Expired && c.Expires.ToUniversalTime() > DateTime.UtcNow);

    /// <summary>
    /// 构造函数，初始化认证设置视图模型
    /// </summary>
    /// <param name="settingsService">设置服务，用于存储和读取认证信息</param>
    public AuthSetupViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;

        // 监听LastAuthCookies属性变化，当变化时更新相关UI属性
        _eventRoot.Add(
            _settingsService.WatchProperty(
                o => o.LastAuthCookies,
                () =>
                {
                    OnPropertyChanged(nameof(Cookies));
                    OnPropertyChanged(nameof(IsAuthenticated));
                }
            )
        );
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
