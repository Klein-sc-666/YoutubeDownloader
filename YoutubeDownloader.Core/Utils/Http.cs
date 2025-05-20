using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace YoutubeDownloader.Core.Utils;

/// <summary>
/// HTTP工具类，提供全局共享的HTTP客户端实例
/// </summary>
public static class Http
{
    /// <summary>
    /// 全局共享的HTTP客户端实例
    /// 配置了用户代理头，标识为YoutubeDownloader应用程序
    /// </summary>
    public static HttpClient Client { get; } =
        new()
        {
            DefaultRequestHeaders =
            {
                // 设置用户代理头，某些服务需要此信息来识别客户端
                UserAgent =
                {
                    new ProductInfoHeaderValue(
                        "YoutubeDownloader",
                        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)
                    ),
                },
            },
        };
}
