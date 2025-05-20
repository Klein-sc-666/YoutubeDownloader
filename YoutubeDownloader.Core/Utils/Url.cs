using System.Text.RegularExpressions;
using YoutubeDownloader.Core.Utils.Extensions;

namespace YoutubeDownloader.Core.Utils;

/// <summary>
/// URL处理工具类，提供URL相关的辅助方法
/// </summary>
public static class Url
{
    /// <summary>
    /// 尝试从URL中提取文件名
    /// 使用正则表达式匹配URL中最后一个斜杠后、问号前的内容作为文件名
    /// </summary>
    /// <param name="url">需要处理的URL</param>
    /// <returns>提取的文件名，如果提取失败则返回null</returns>
    public static string? TryExtractFileName(string url) =>
        Regex.Match(url, @".+/([^?]*)").Groups[1].Value.NullIfEmptyOrWhiteSpace();
}
