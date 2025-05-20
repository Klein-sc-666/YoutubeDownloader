using System.IO;

namespace YoutubeDownloader.Utils;

/// <summary>
/// 目录操作扩展工具类，提供目录相关的辅助方法
/// </summary>
internal static class DirectoryEx
{
    /// <summary>
    /// 为指定的文件路径创建必要的目录结构
    /// 如果文件所在的目录不存在，则创建该目录
    /// </summary>
    /// <param name="filePath">文件的完整路径</param>
    public static void CreateDirectoryForFile(string filePath)
    {
        var dirPath = Path.GetDirectoryName(filePath);
        if (string.IsNullOrWhiteSpace(dirPath))
            return;

        Directory.CreateDirectory(dirPath);
    }
}
