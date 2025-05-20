using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YoutubeDownloader.Core.Utils;

/// <summary>
/// 路径处理工具类，提供文件路径相关的辅助方法
/// </summary>
public static class PathEx
{
    /// <summary>
    /// 无效文件名字符集合，包含所有系统不允许用于文件名的字符
    /// </summary>
    private static readonly HashSet<char> InvalidFileNameChars =
    [
        .. Path.GetInvalidFileNameChars(),
    ];

    /// <summary>
    /// 转义文件名，将不合法的字符替换为下划线
    /// </summary>
    /// <param name="path">需要转义的文件名或路径</param>
    /// <returns>转义后的安全文件名</returns>
    public static string EscapeFileName(string path)
    {
        var buffer = new StringBuilder(path.Length);

        foreach (var c in path)
            buffer.Append(!InvalidFileNameChars.Contains(c) ? c : '_');

        return buffer.ToString();
    }
}
