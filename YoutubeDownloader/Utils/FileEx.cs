using System.IO;

namespace YoutubeDownloader.Utils;

/// <summary>
/// 文件路径扩展工具类，提供文件路径相关的辅助方法
/// </summary>
internal static class PathEx
{
    /// <summary>
    /// 确保文件路径的唯一性，如果文件已存在则通过添加序号来创建唯一路径
    /// </summary>
    /// <param name="baseFilePath">基础文件路径</param>
    /// <param name="maxRetries">最大重试次数，默认为100</param>
    /// <returns>唯一的文件路径，如果无法创建唯一路径则返回原始路径</returns>
    public static string EnsureUniquePath(string baseFilePath, int maxRetries = 100)
    {
        // 如果文件不存在，直接返回原始路径
        if (!File.Exists(baseFilePath))
            return baseFilePath;

        // 分解文件路径为目录、文件名（不含扩展名）和扩展名
        var baseDirPath = Path.GetDirectoryName(baseFilePath);
        var baseFileNameWithoutExtension = Path.GetFileNameWithoutExtension(baseFilePath);
        var baseFileExtension = Path.GetExtension(baseFilePath);

        // 尝试添加序号来创建唯一路径
        for (var i = 1; i <= maxRetries; i++)
        {
            // 构建新的文件名，格式为"原文件名 (序号).扩展名"
            var fileName = $"{baseFileNameWithoutExtension} ({i}){baseFileExtension}";

            // 构建完整路径，如果目录为空则只使用文件名
            var filePath = !string.IsNullOrWhiteSpace(baseDirPath)
                ? Path.Combine(baseDirPath, fileName)
                : fileName;

            // 如果新路径不存在，则返回该路径
            if (!File.Exists(filePath))
                return filePath;
        }

        // 如果达到最大重试次数仍未找到唯一路径，则返回原始路径
        return baseFilePath;
    }
}
