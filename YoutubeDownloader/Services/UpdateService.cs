using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Onova;
using Onova.Exceptions;
using Onova.Services;
using YoutubeDownloader.Core.Downloading;

namespace YoutubeDownloader.Services;

/// <summary>
/// 应用程序更新服务，负责检查、准备和应用软件更新
/// </summary>
/// <param name="settingsService">设置服务，用于获取用户的更新偏好设置</param>
public class UpdateService(SettingsService settingsService) : IDisposable
{
    /// <summary>
    /// 更新管理器实例，仅在Windows平台上初始化
    /// 根据是否捆绑FFmpeg选择不同的更新包
    /// </summary>
    private readonly IUpdateManager? _updateManager = OperatingSystem.IsWindows()
        ? new UpdateManager(
            new GithubPackageResolver(
                "Tyrrrz",
                "YoutubeDownloader",
                // Examples:
                // YoutubeDownloader.win-arm64.zip
                // YoutubeDownloader.win-x64.zip
                // YoutubeDownloader.linux-x64.zip
                // YoutubeDownloader.Bare.linux-x64.zip
                FFmpeg.IsBundled()
                    ? $"YoutubeDownloader.{RuntimeInformation.RuntimeIdentifier}.zip"
                    : $"YoutubeDownloader.Bare.{RuntimeInformation.RuntimeIdentifier}.zip"
            ),
            new ZipPackageExtractor()
        )
        : null;

    /// <summary>
    /// 存储检测到的更新版本号
    /// </summary>
    private Version? _updateVersion;

    /// <summary>
    /// 标记更新是否已准备完成
    /// </summary>
    private bool _updatePrepared;

    /// <summary>
    /// 标记更新程序是否已启动
    /// </summary>
    private bool _updaterLaunched;

    /// <summary>
    /// 检查是否有可用的更新
    /// </summary>
    /// <returns>如果有可用更新，返回更新版本号；否则返回null</returns>
    public async Task<Version?> CheckForUpdatesAsync()
    {
        if (_updateManager is null)
            return null;

        if (!settingsService.IsAutoUpdateEnabled)
            return null;

        var check = await _updateManager.CheckForUpdatesAsync();
        return check.CanUpdate ? check.LastVersion : null;
    }

    /// <summary>
    /// 准备指定版本的更新，下载并解压更新包
    /// </summary>
    /// <param name="version">要准备的更新版本</param>
    public async Task PrepareUpdateAsync(Version version)
    {
        if (_updateManager is null)
            return;

        if (!settingsService.IsAutoUpdateEnabled)
            return;

        try
        {
            await _updateManager.PrepareUpdateAsync(_updateVersion = version);
            _updatePrepared = true;
        }
        catch (UpdaterAlreadyLaunchedException)
        {
            // Ignore race conditions
        }
        catch (LockFileNotAcquiredException)
        {
            // Ignore race conditions
        }
    }

    /// <summary>
    /// 完成更新过程，启动更新程序应用更新
    /// </summary>
    /// <param name="needRestart">更新后是否需要重启应用</param>
    public void FinalizeUpdate(bool needRestart)
    {
        if (_updateManager is null)
            return;

        if (!settingsService.IsAutoUpdateEnabled)
            return;

        if (_updateVersion is null || !_updatePrepared || _updaterLaunched)
            return;

        try
        {
            _updateManager.LaunchUpdater(_updateVersion, needRestart);
            _updaterLaunched = true;
        }
        catch (UpdaterAlreadyLaunchedException)
        {
            // Ignore race conditions
        }
        catch (LockFileNotAcquiredException)
        {
            // Ignore race conditions
        }
    }

    /// <summary>
    /// 释放更新管理器资源
    /// </summary>
    public void Dispose() => _updateManager?.Dispose();
}
