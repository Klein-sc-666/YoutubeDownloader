namespace YoutubeDownloader.Core.Utils.Extensions;

/// <summary>
/// 字符串扩展类，提供处理字符串的辅助方法
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// 如果字符串为空或仅包含空白字符，则返回null，否则返回原字符串
    /// 用于将空字符串或空白字符串规范化为null，简化空值检查
    /// </summary>
    /// <param name="str">要检查的字符串</param>
    /// <returns>原字符串或null</returns>
    public static string? NullIfEmptyOrWhiteSpace(this string str) =>
        !string.IsNullOrEmpty(str.Trim()) ? str : null;
}
