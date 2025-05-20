using System;
using System.Linq;
using Avalonia.Interactivity;
using Avalonia.WebView.Windows.Core;
using Microsoft.Web.WebView2.Core;
using WebViewCore.Events;
using YoutubeDownloader.Framework;
using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.Views.Dialogs;

public partial class AuthSetupView : UserControl<AuthSetupViewModel>
{
    /// <summary>
    /// YouTube主页URL常量
    /// </summary>
    private const string HomePageUrl = "https://www.youtube.com";

    /// <summary>
    /// Google登录页URL，登录成功后重定向到YouTube主页
    /// </summary>
    private static readonly string LoginPageUrl =
        $"https://accounts.google.com/ServiceLogin?continue={Uri.EscapeDataString(HomePageUrl)}";

    /// <summary>
    /// WebView2核心控件引用
    /// </summary>
    private CoreWebView2? _coreWebView2;

    /// <summary>
    /// 构造函数，初始化控件
    /// </summary>
    public AuthSetupView() => InitializeComponent();

    /// <summary>
    /// 导航到登录页面
    /// </summary>
    private void NavigateToLoginPage() => WebBrowser.Url = new Uri(LoginPageUrl);

    /// <summary>
    /// 登出按钮点击事件处理方法
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="args">路由事件参数</param>
    private void LogOutButton_OnClick(object sender, RoutedEventArgs args)
    {
        DataContext.Cookies = null;
        NavigateToLoginPage();
    }

    /// <summary>
    /// WebBrowser加载完成事件处理方法
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="args">路由事件参数</param>
    private void WebBrowser_OnLoaded(object sender, RoutedEventArgs args) => NavigateToLoginPage();

    /// <summary>
    /// WebView创建完成事件处理方法，配置WebView2的设置
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="args">WebView创建事件参数</param>
    private void WebBrowser_OnWebViewCreated(object sender, WebViewCreatedEventArgs args)
    {
        if (!args.IsSucceed)
            return;

        var platformWebView = WebBrowser.PlatformWebView as WebView2Core;
        var coreWebView2 = platformWebView?.CoreWebView2;

        if (coreWebView2 is null)
            return;

        coreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        coreWebView2.Settings.AreDevToolsEnabled = false;
        coreWebView2.Settings.IsGeneralAutofillEnabled = false;
        coreWebView2.Settings.IsPasswordAutosaveEnabled = false;
        coreWebView2.Settings.IsStatusBarEnabled = false;
        coreWebView2.Settings.IsSwipeNavigationEnabled = false;

        _coreWebView2 = coreWebView2;
    }

    /// <summary>
    /// WebBrowser导航开始事件处理方法，管理Cookie和登录状态
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="args">WebView URL加载事件参数</param>
    private async void WebBrowser_OnNavigationStarting(
        object? sender,
        WebViewUrlLoadingEventArg args
    )
    {
        if (_coreWebView2 is null)
            return;

        // 当用户尝试再次登录时，重置现有的浏览器Cookie
        if (string.Equals(args.Url?.AbsoluteUri, LoginPageUrl, StringComparison.OrdinalIgnoreCase))
            _coreWebView2.CookieManager.DeleteAllCookies();

        // 在重定向到主页后提取Cookie（即登录后）
        if (
            args.Url?.AbsoluteUri.StartsWith(HomePageUrl, StringComparison.OrdinalIgnoreCase)
            == true
        )
        {
            var cookies = await _coreWebView2!.CookieManager.GetCookiesAsync(args.Url.AbsoluteUri);
            DataContext.Cookies = cookies.Select(c => c.ToSystemNetCookie()).ToArray();
        }
    }
}
