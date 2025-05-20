using Avalonia.Controls;
using Avalonia.Controls.Templates;
using YoutubeDownloader.ViewModels;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.Views;
using YoutubeDownloader.Views.Components;
using YoutubeDownloader.Views.Dialogs;

namespace YoutubeDownloader.Framework;

public partial class ViewManager
{
    /// <summary>
    /// 尝试创建View并将其与ViewModel绑定
    /// </summary>
    /// <param name="viewModel">需要绑定的ViewModel实例</param>
    /// <returns>已绑定DataContext的View控件，如果没有匹配的View则返回null</returns>
    public Control? TryBindView(ViewModelBase viewModel)
    {
        var view = TryCreateView(viewModel);
        if (view is null)
            return null;

        // 如果View的DataContext为null，则设置为传入的viewModel
        view.DataContext ??= viewModel;

        return view;
    }

    /// <summary>
    /// 尝试为指定的ViewModel创建对应的View控件
    /// </summary>
    /// <param name="viewModel">需要创建View的ViewModel实例</param>
    /// <returns>对应的View控件实例，如果没有匹配的View则返回null</returns>
    private Control? TryCreateView(ViewModelBase viewModel) =>
        viewModel switch
        {
            MainViewModel => new MainView(), // 主视图
            DashboardViewModel => new DashboardView(), // 仪表板视图
            AuthSetupViewModel => new AuthSetupView(), // 认证设置视图
            DownloadMultipleSetupViewModel => new DownloadMultipleSetupView(), // 多视频下载设置视图
            DownloadSingleSetupViewModel => new DownloadSingleSetupView(), // 单视频下载设置视图
            MessageBoxViewModel => new MessageBoxView(), // 消息框视图
            SettingsViewModel => new SettingsView(), // 设置视图
            _ => null, // 未知ViewModel类型返回null
        };
}

/// <summary>
/// ViewManager的IDataTemplate实现部分，用于在XAML中自动将ViewModel转换为对应的View
/// </summary>
public partial class ViewManager : IDataTemplate
{
    /// <summary>
    /// 判断数据对象是否可以由此模板处理
    /// </summary>
    /// <param name="data">要检查的数据对象</param>
    /// <returns>如果数据是ViewModelBase类型则返回true，否则返回false</returns>
    bool IDataTemplate.Match(object? data) => data is ViewModelBase;

    /// <summary>
    /// 根据数据对象构建对应的UI控件
    /// </summary>
    /// <param name="data">用于构建UI的数据对象</param>
    /// <returns>与数据对象对应的UI控件，如果无法构建则返回null</returns>
    Control? ITemplate<object?, Control?>.Build(object? data) =>
        data is ViewModelBase viewModel ? TryBindView(viewModel) : null;
}
