using System;
using YoutubeDownloader.Core.Utils;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Core.Downloading;

/// <summary>
/// 文件名模板工具类，用于根据模板和视频信息生成下载文件的名称
/// </summary>
public static class FileNameTemplate
{
    /// <summary>
    /// 应用文件名模板，将模板中的占位符替换为视频的实际信息
    /// </summary>
    /// <param name="template">文件名模板字符串，包含各种占位符</param>
    /// <param name="video">视频信息对象</param>
    /// <param name="container">视频容器格式</param>
    /// <param name="number">可选的编号，用于批量下载时区分文件</param>
    /// <returns>处理后的文件名，已转义非法字符并添加文件扩展名</returns>
    /// <remarks>
    /// 支持的占位符：
    /// $numc - 纯数字编号（如果提供）
    /// $num - 带方括号的编号，如[1]（如果提供）
    /// $id - 视频ID
    /// $title - 视频标题
    /// $author - 视频作者/频道名称
    /// $uploadDate - 视频上传日期（格式：yyyy-MM-dd）
    /// </remarks>
    public static string Apply(
        string template,
        IVideo video,
        Container container,
        string? number = null
    ) =>
        PathEx.EscapeFileName(
            template
                .Replace("$numc", number ?? "", StringComparison.Ordinal)
                .Replace("$num", number is not null ? $"[{number}]" : "", StringComparison.Ordinal)
                .Replace("$id", video.Id, StringComparison.Ordinal)
                .Replace("$title", video.Title, StringComparison.Ordinal)
                .Replace("$author", video.Author.ChannelTitle, StringComparison.Ordinal)
                .Replace(
                    "$uploadDate",
                    (video as Video)?.UploadDate.ToString("yyyy-MM-dd") ?? "",
                    StringComparison.Ordinal
                )
                .Trim()
                + '.'
                + container.Name
        );
}
