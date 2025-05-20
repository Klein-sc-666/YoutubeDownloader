using System;

namespace YoutubeDownloader.Utils;

/// <summary>
/// 可释放资源工具类，提供创建简单IDisposable对象的方法
/// 用于在不需要创建完整类的情况下实现资源释放功能
/// </summary>
/// <param name="dispose">释放资源时要执行的操作</param>
internal class Disposable(Action dispose) : IDisposable
{
    /// <summary>
    /// 创建一个新的IDisposable实例，指定释放时要执行的操作
    /// </summary>
    /// <param name="dispose">释放资源时要执行的操作</param>
    /// <returns>可释放的对象实例</returns>
    public static IDisposable Create(Action dispose) => new Disposable(dispose);

    /// <summary>
    /// 执行资源释放操作
    /// </summary>
    public void Dispose() => dispose();
}
