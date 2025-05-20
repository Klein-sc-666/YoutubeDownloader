using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Core.Utils;
using YoutubeDownloader.Core.Utils.Extensions;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Tagging;

/// <summary>
/// 媒体标签注入器，负责将YouTube视频的相关信息注入到下载的媒体文件中
/// </summary>
public class MediaTagInjector
{
    /// <summary>
    /// MusicBrainz客户端实例，用于查询音乐相关的元数据
    /// </summary>
    private readonly MusicBrainzClient _musicBrainz = new();

    /// <summary>
    /// 注入杂项元数据，包括视频描述和下载信息
    /// </summary>
    /// <param name="mediaFile">目标媒体文件</param>
    /// <param name="video">YouTube视频信息</param>
    private void InjectMiscMetadata(MediaFile mediaFile, IVideo video)
    {
        // 如果视频有描述，则设置到媒体文件的描述字段
        var description = (video as Video)?.Description;
        if (!string.IsNullOrWhiteSpace(description))
            mediaFile.SetDescription(description);

        // 设置评论字段，包含下载信息和视频来源信息
        mediaFile.SetComment(
            $"""
            Downloaded using YoutubeDownloader (https://github.com/Tyrrrz/YoutubeDownloader)
            Video: {video.Title}
            Video URL: {video.Url}
            Channel: {video.Author.ChannelTitle}
            Channel URL: {video.Author.ChannelUrl}
            """
        );
    }

    /// <summary>
    /// 尝试从MusicBrainz查询并注入音乐相关的元数据
    /// </summary>
    /// <param name="mediaFile">目标媒体文件</param>
    /// <param name="video">YouTube视频信息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步任务</returns>
    private async Task InjectMusicMetadataAsync(
        MediaFile mediaFile,
        IVideo video,
        CancellationToken cancellationToken = default
    )
    {
        // 使用视频标题在MusicBrainz搜索相关音乐记录
        var recordings = await _musicBrainz.SearchRecordingsAsync(video.Title, cancellationToken);

        // 查找最匹配的音乐记录：
        // 1. 记录标题必须是视频标题的一部分
        // 2. 记录艺术家必须是视频标题或频道名称的一部分
        var recording = recordings.FirstOrDefault(r =>
            video.Title.Contains(r.Title, StringComparison.OrdinalIgnoreCase)
            && (
                video.Title.Contains(r.Artist, StringComparison.OrdinalIgnoreCase)
                || video.Author.ChannelTitle.Contains(r.Artist, StringComparison.OrdinalIgnoreCase)
            )
        );

        // 如果没有找到匹配的记录，则不注入音乐元数据
        if (recording is null)
            return;

        // 设置艺术家和标题
        mediaFile.SetArtist(recording.Artist);
        mediaFile.SetTitle(recording.Title);

        // 如果有艺术家排序信息，则设置
        if (!string.IsNullOrWhiteSpace(recording.ArtistSort))
            mediaFile.SetArtistSort(recording.ArtistSort);

        // 如果有专辑信息，则设置
        if (!string.IsNullOrWhiteSpace(recording.Album))
            mediaFile.SetAlbum(recording.Album);
    }

    /// <summary>
    /// 下载并注入视频缩略图作为媒体文件的封面
    /// </summary>
    /// <param name="mediaFile">目标媒体文件</param>
    /// <param name="video">YouTube视频信息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步任务</returns>
    private async Task InjectThumbnailAsync(
        MediaFile mediaFile,
        IVideo video,
        CancellationToken cancellationToken = default
    )
    {
        // 选择最高分辨率的JPG格式缩略图，如果没有则使用默认缩略图URL
        var thumbnailUrl =
            video
                .Thumbnails.Where(t =>
                    string.Equals(t.TryGetImageFormat(), "jpg", StringComparison.OrdinalIgnoreCase)
                )
                .OrderByDescending(t => t.Resolution.Area)
                .Select(t => t.Url)
                .FirstOrDefault() ?? $"https://i.ytimg.com/vi/{video.Id}/hqdefault.jpg";

        // 下载缩略图并设置到媒体文件
        mediaFile.SetThumbnail(
            await Http.Client.GetByteArrayAsync(thumbnailUrl, cancellationToken)
        );
    }

    /// <summary>
    /// 将YouTube视频的所有相关信息注入到指定的媒体文件中
    /// </summary>
    /// <param name="filePath">媒体文件路径</param>
    /// <param name="video">YouTube视频信息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步任务</returns>
    public async Task InjectTagsAsync(
        string filePath,
        IVideo video,
        CancellationToken cancellationToken = default
    )
    {
        // 打开媒体文件并使用using语句确保资源正确释放
        using var mediaFile = MediaFile.Open(filePath);

        // 注入各类元数据
        InjectMiscMetadata(mediaFile, video);
        await InjectMusicMetadataAsync(mediaFile, video, cancellationToken);
        await InjectThumbnailAsync(mediaFile, video, cancellationToken);

        // 保存更改
        mediaFile.Save();
    }
}
