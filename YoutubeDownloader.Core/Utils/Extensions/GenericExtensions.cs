using System;

namespace YoutubeDownloader.Core.Utils.Extensions;

/// <summary>
/// 通用扩展类，提供适用于任何类型的扩展方法
/// </summary>
public static class GenericExtensions
{
    /// <summary>
    /// 管道操作符实现，将输入值传递给转换函数并返回结果
    /// 允许链式调用转换函数，类似于函数式编程中的管道操作
    /// </summary>
    /// <typeparam name="TIn">输入值类型</typeparam>
    /// <typeparam name="TOut">输出值类型</typeparam>
    /// <param name="input">输入值</param>
    /// <param name="transform">转换函数</param>
    /// <returns>转换后的结果</returns>
    /// <example>
    /// var result = "123".Pipe(int.Parse).Pipe(x => x * 2);
    /// // result 等于 246
    /// </example>
    public static TOut Pipe<TIn, TOut>(this TIn input, Func<TIn, TOut> transform) =>
        transform(input);
}
