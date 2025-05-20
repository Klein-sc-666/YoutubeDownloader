using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace YoutubeDownloader.Framework;

/// <summary>
/// 视图模型基类，提供属性变更通知和资源释放功能
/// </summary>
public abstract class ViewModelBase : ObservableObject, IDisposable
{
    /// <summary>
    /// 析构函数，调用非托管资源的释放方法
    /// </summary>
    ~ViewModelBase() => Dispose(false);

    /// <summary>
    /// 触发所有属性的变更通知
    /// </summary>
    protected void OnAllPropertiesChanged() => OnPropertyChanged(string.Empty);

    /// <summary>
    /// 释放资源的虚方法，可由派生类重写以实现自定义资源释放逻辑
    /// </summary>
    /// <param name="disposing">是否为显式释放，true表示由Dispose方法调用，false表示由终结器调用</param>
    protected virtual void Dispose(bool disposing) { }

    /// <summary>
    /// 实现IDisposable接口的Dispose方法，释放资源并抑制终结器
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
