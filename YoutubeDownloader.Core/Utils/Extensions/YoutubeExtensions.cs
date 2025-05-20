using System.IO;
using YoutubeExplode.Common;

namespace YoutubeDownloader.Core.Utils.Extensions;

/// <summary>
/// YouTube相关扩展类，提供处理YouTube数据的辅助方法
/// </summary>
public static class YoutubeExtensions
{
    /// <summary>
    /// 尝试获取缩略图的图像格式
    /// 从缩略图URL中提取文件名，然后获取其扩展名作为图像格式
    /// </summary>
    /// <param name="thumbnail">YouTube缩略图对象</param>
    /// <returns>图像格式字符串（如"jpg"、"png"等），如果无法确定则返回null</returns>
    public static string? TryGetImageFormat(this Thumbnail thumbnail) =>
        Url.TryExtractFileName(thumbnail.Url)?.Pipe(Path.GetExtension)?.Trim('.');
}
