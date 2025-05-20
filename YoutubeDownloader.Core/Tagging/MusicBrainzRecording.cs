namespace YoutubeDownloader.Core.Tagging;

/// <summary>
/// MusicBrainz音乐记录类，表示从MusicBrainz API获取的音乐元数据
/// </summary>
/// <param name="Artist">艺术家名称</param>
/// <param name="ArtistSort">艺术家排序名称，用于按艺术家名称排序时使用（可为空）</param>
/// <param name="Title">音乐标题</param>
/// <param name="Album">专辑名称（可为空）</param>
internal record MusicBrainzRecording(
    string Artist,
    string? ArtistSort,
    string Title,
    string? Album
);
