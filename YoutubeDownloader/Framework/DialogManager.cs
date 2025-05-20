using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform.Storage;
using DialogHostAvalonia;
using YoutubeDownloader.Utils.Extensions;

namespace YoutubeDownloader.Framework;

public class DialogManager : IDisposable
{
    private readonly SemaphoreSlim _dialogLock = new(1, 1);

    public async Task<T?> ShowDialogAsync<T>(DialogViewModelBase<T> dialog)
    {
        await _dialogLock.WaitAsync();
        try
        {
            await DialogHost.Show(
                dialog,
                // It's fine to await in a void method here because it's an event handler
                // ReSharper disable once AsyncVoidLambda
                async (object _, DialogOpenedEventArgs args) =>
                {
                    await dialog.WaitForCloseAsync();

                    try
                    {
                        args.Session.Close();
                    }
                    catch (InvalidOperationException)
                    {
                        // Dialog host is already processing a close operation
                    }
                }
            );

            return dialog.DialogResult;
        }
        finally
        {
            _dialogLock.Release();
        }
    }

    /// <summary>
    /// 显示文件保存对话框，让用户选择文件保存位置和名称
    /// </summary>
    /// <param name="fileTypes">可选的文件类型过滤器列表，用于限制可选择的文件类型</param>
    /// <param name="defaultFilePath">默认的文件路径和名称，作为初始建议值</param>
    /// <returns>用户选择的文件路径，如果用户取消则返回null</returns>
    public async Task<string?> PromptSaveFilePathAsync(
        IReadOnlyList<FilePickerFileType>? fileTypes = null,
        string defaultFilePath = ""
    )
    {
        // 获取应用程序的顶级视觉元素，用于显示对话框
        // 如果找不到顶级元素，则抛出异常
        var topLevel =
            Application.Current?.ApplicationLifetime?.TryGetTopLevel()
            ?? throw new ApplicationException("Could not find the top-level visual element.");

        // 使用Avalonia的存储提供器API显示保存文件对话框
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                // 设置可选择的文件类型
                FileTypeChoices = fileTypes,
                // 设置建议的文件名
                SuggestedFileName = defaultFilePath,
                // 设置默认文件扩展名（从defaultFilePath中提取并去除前导点）
                DefaultExtension = Path.GetExtension(defaultFilePath).TrimStart('.'),
            }
        );

        // 返回用户选择的文件路径，如果用户取消则返回null
        return file?.Path.LocalPath;
    }

    public async Task<string?> PromptDirectoryPathAsync(string defaultDirPath = "")
    {
        var topLevel =
            Application.Current?.ApplicationLifetime?.TryGetTopLevel()
            ?? throw new ApplicationException("Could not find the top-level visual element.");

        var startLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(
            defaultDirPath
        );

        var folderPickResult = await topLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                SuggestedStartLocation = startLocation,
            }
        );

        return folderPickResult.FirstOrDefault()?.Path.LocalPath;
    }

    public void Dispose() => _dialogLock.Dispose();
}
