using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace YoutubeDownloader.Utils.Extensions;

/// <summary>
/// 属性变更通知扩展工具类，提供监听INotifyPropertyChanged对象属性变化的辅助方法
/// </summary>
internal static class NotifyPropertyChangedExtensions
{
    /// <summary>
    /// 监听指定对象的特定属性变化
    /// </summary>
    /// <typeparam name="TOwner">属性所有者类型</typeparam>
    /// <typeparam name="TProperty">属性类型</typeparam>
    /// <param name="owner">属性所有者对象</param>
    /// <param name="propertyExpression">指定要监听的属性的表达式</param>
    /// <param name="callback">属性变化时要执行的回调方法</param>
    /// <param name="watchInitialValue">是否在开始监听时立即执行一次回调，默认为false</param>
    /// <returns>可释放对象，用于停止监听</returns>
    /// <exception cref="ArgumentException">如果提供的表达式不引用属性</exception>
    public static IDisposable WatchProperty<TOwner, TProperty>(
        this TOwner owner,
        Expression<Func<TOwner, TProperty>> propertyExpression,
        Action callback,
        bool watchInitialValue = false
    )
        where TOwner : INotifyPropertyChanged
    {
        // 从表达式中提取属性信息
        var memberExpression = propertyExpression.Body as MemberExpression;
        if (memberExpression?.Member is not PropertyInfo property)
            throw new ArgumentException("提供的表达式必须引用一个属性。");

        // 属性变化事件处理方法
        void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            // 如果属性名为空（表示所有属性都变化了）或者与监听的属性名匹配，则执行回调
            if (
                string.IsNullOrWhiteSpace(args.PropertyName)
                || string.Equals(args.PropertyName, property.Name, StringComparison.Ordinal)
            )
            {
                callback();
            }
        }

        // 订阅属性变化事件
        owner.PropertyChanged += OnPropertyChanged;

        // 如果需要监听初始值，则立即执行一次回调
        if (watchInitialValue)
            callback();

        // 返回可释放对象，用于取消事件订阅
        return Disposable.Create(() => owner.PropertyChanged -= OnPropertyChanged);
    }

    /// <summary>
    /// 监听指定对象的所有属性变化
    /// </summary>
    /// <typeparam name="TOwner">属性所有者类型</typeparam>
    /// <param name="owner">属性所有者对象</param>
    /// <param name="callback">任何属性变化时要执行的回调方法</param>
    /// <param name="watchInitialValues">是否在开始监听时立即执行一次回调，默认为false</param>
    /// <returns>可释放对象，用于停止监听</returns>
    public static IDisposable WatchAllProperties<TOwner>(
        this TOwner owner,
        Action callback,
        bool watchInitialValues = false
    )
        where TOwner : INotifyPropertyChanged
    {
        // 属性变化事件处理方法，无论哪个属性变化都执行回调
        void OnPropertyChanged(object? sender, PropertyChangedEventArgs args) => callback();

        // 订阅属性变化事件
        owner.PropertyChanged += OnPropertyChanged;

        // 如果需要监听初始值，则立即执行一次回调
        if (watchInitialValues)
            callback();

        // 返回可释放对象，用于取消事件订阅
        return Disposable.Create(() => owner.PropertyChanged -= OnPropertyChanged);
    }
}
