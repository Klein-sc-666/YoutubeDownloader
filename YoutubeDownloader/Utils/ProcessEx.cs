using System.Collections.Generic;
using System.Diagnostics;

namespace YoutubeDownloader.Utils;

/// <summary>
/// 进程操作扩展工具类，提供启动外部进程的辅助方法
/// </summary>
internal static class ProcessEx
{
    /// <summary>
    /// 启动外部进程
    /// </summary>
    /// <param name="path">要启动的程序或文件的路径</param>
    /// <param name="arguments">命令行参数列表，默认为null</param>
    public static void Start(string path, IReadOnlyList<string>? arguments = null)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo(path);

        // 如果提供了参数，则添加到进程的参数列表中
        if (arguments is not null)
        {
            foreach (var argument in arguments)
                process.StartInfo.ArgumentList.Add(argument);
        }

        process.Start();
    }

    /// <summary>
    /// 使用Shell执行方式启动外部进程
    /// 这种方式会使用系统默认的程序打开指定的文件
    /// </summary>
    /// <param name="path">要打开的文件路径</param>
    /// <param name="arguments">命令行参数列表，默认为null</param>
    public static void StartShellExecute(string path, IReadOnlyList<string>? arguments = null)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo(path) { UseShellExecute = true };

        // 如果提供了参数，则添加到进程的参数列表中
        if (arguments is not null)
        {
            foreach (var argument in arguments)
                process.StartInfo.ArgumentList.Add(argument);
        }

        process.Start();
    }
}
