using System;
using TagLib;
using TagFile = TagLib.File;

namespace YoutubeDownloader.Core.Tagging;

/// <summary>
/// 媒体文件类，封装了对音视频文件元数据标签的操作
/// 实现IDisposable接口以确保正确释放底层TagLib文件资源
/// </summary>
/// <param name="file">TagLib文件对象，用于读写媒体文件的元数据</param>
internal partial class MediaFile(TagFile file) : IDisposable
{
    /// <summary>
    /// 设置媒体文件的缩略图/封面图
    /// </summary>
    /// <param name="thumbnailData">缩略图的二进制数据</param>
    public void SetThumbnail(byte[] thumbnailData) =>
        file.Tag.Pictures = [new Picture(thumbnailData)];

    /// <summary>
    /// 设置媒体文件的艺术家信息
    /// </summary>
    /// <param name="artist">艺术家名称</param>
    public void SetArtist(string artist) => file.Tag.Performers = [artist];

    /// <summary>
    /// 设置媒体文件的艺术家排序信息（用于按艺术家名称排序时使用）
    /// </summary>
    /// <param name="artistSort">艺术家排序名称</param>
    public void SetArtistSort(string artistSort) => file.Tag.PerformersSort = [artistSort];

    /// <summary>
    /// 设置媒体文件的标题
    /// </summary>
    /// <param name="title">标题文本</param>
    public void SetTitle(string title) => file.Tag.Title = title;

    /// <summary>
    /// 设置媒体文件的专辑信息
    /// </summary>
    /// <param name="album">专辑名称</param>
    public void SetAlbum(string album) => file.Tag.Album = album;

    /// <summary>
    /// 设置媒体文件的描述信息
    /// </summary>
    /// <param name="description">描述文本</param>
    public void SetDescription(string description) => file.Tag.Description = description;

    /// <summary>
    /// 设置媒体文件的评论信息
    /// </summary>
    /// <param name="comment">评论文本</param>
    public void SetComment(string comment) => file.Tag.Comment = comment;

    /// <summary>
    /// 保存对媒体文件元数据的修改
    /// 同时会更新标签的时间戳为当前时间
    /// </summary>
    public void Save()
    {
        file.Tag.DateTagged = DateTime.Now;
        file.Save();
    }

    /// <summary>
    /// 释放底层TagLib文件资源
    /// </summary>
    public void Dispose() => file.Dispose();
}

/// <summary>
/// MediaFile类的静态工厂方法部分
/// </summary>
internal partial class MediaFile
{
    /// <summary>
    /// 打开指定路径的媒体文件
    /// </summary>
    /// <param name="filePath">媒体文件的完整路径</param>
    /// <returns>MediaFile实例，用于操作该文件的元数据</returns>
    public static MediaFile Open(string filePath) => new(TagFile.Create(filePath));
}
