using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace YoutubeDownloader.Core.Downloading;

/// <summary>
/// FFmpeg工具类，用于定位和验证FFmpeg可执行文件
/// FFmpeg是用于处理音视频的命令行工具，本应用用它来转换和处理下载的视频
/// </summary>
public static class FFmpeg
{
    /// <summary>
    /// 获取FFmpeg可执行文件的名称，根据操作系统返回不同的文件名
    /// Windows系统返回ffmpeg.exe，其他系统返回ffmpeg
    /// </summary>
    private static string CliFileName { get; } =
        OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";

    /// <summary>
    /// 尝试获取FFmpeg可执行文件的完整路径
    /// 搜索顺序：应用程序目录、当前目录、系统PATH环境变量中的目录
    /// </summary>
    /// <returns>FFmpeg可执行文件的完整路径，如果未找到则返回null</returns>
    public static string? TryGetCliFilePath()
    {
        // 获取可能包含FFmpeg的所有目录路径
        static IEnumerable<string> GetProbeDirectoryPaths()
        {
            // 首先检查应用程序基目录
            yield return AppContext.BaseDirectory;
            // 然后检查当前工作目录
            yield return Directory.GetCurrentDirectory();

            // 检查进程PATH环境变量中的所有目录
            if (
                Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) is
                { } processPaths
            )
            {
                foreach (var path in processPaths)
                    yield return path;
            }

            // 在Windows系统上，还检查注册表中的PATH变量
            if (OperatingSystem.IsWindows())
            {
                // 检查用户级PATH环境变量
                if (
                    Environment
                        .GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User)
                        ?.Split(Path.PathSeparator) is
                    { } userPaths
                )
                {
                    foreach (var path in userPaths)
                        yield return path;
                }

                // 检查系统级PATH环境变量
                if (
                    Environment
                        .GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine)
                        ?.Split(Path.PathSeparator) is
                    { } systemPaths
                )
                {
                    foreach (var path in systemPaths)
                        yield return path;
                }
            }
        }

        // 从所有可能的目录中查找FFmpeg可执行文件
        return GetProbeDirectoryPaths()
            .Distinct(StringComparer.Ordinal) // 去除重复路径
            .Select(dirPath => Path.Combine(dirPath, CliFileName)) // 组合完整文件路径
            .FirstOrDefault(File.Exists); // 返回第一个存在的文件路径
    }

    /// <summary>
    /// 检查应用程序是否捆绑了FFmpeg
    /// </summary>
    /// <returns>如果应用程序目录中存在FFmpeg可执行文件，则返回true</returns>
    public static bool IsBundled() =>
        File.Exists(Path.Combine(AppContext.BaseDirectory, CliFileName));

    /// <summary>
    /// 检查系统中是否可用FFmpeg
    /// </summary>
    /// <returns>如果能找到FFmpeg可执行文件，则返回true</returns>
    public static bool IsAvailable() => !string.IsNullOrWhiteSpace(TryGetCliFilePath());
}
