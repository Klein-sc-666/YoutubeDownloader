namespace YoutubeDownloader.ViewModels.Components;

/// <summary>
/// 表示下载任务的状态枚举
/// </summary>
public enum DownloadStatus
{
    /// <summary>
    /// 已加入队列，等待下载
    /// </summary>
    Enqueued,

    /// <summary>
    /// 已开始下载
    /// </summary>
    Started,

    /// <summary>
    /// 下载已完成
    /// </summary>
    Completed,

    /// <summary>
    /// 下载失败
    /// </summary>
    Failed,

    /// <summary>
    /// 下载被取消
    /// </summary>
    Canceled,
}
