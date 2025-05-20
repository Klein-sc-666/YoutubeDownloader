using System.Runtime.InteropServices;

namespace YoutubeDownloader.Utils;

/// <summary>
/// 原生方法调用类，提供对操作系统API的直接调用
/// 用于实现平台特定的功能，如Windows消息框等
/// </summary>
internal static class NativeMethods
{
    /// <summary>
    /// Windows平台特定的API调用
    /// </summary>
    public static class Windows
    {
        /// <summary>
        /// 调用Windows系统的MessageBox函数显示消息对话框
        /// </summary>
        /// <param name="hWnd">父窗口句柄</param>
        /// <param name="text">消息文本内容</param>
        /// <param name="caption">对话框标题</param>
        /// <param name="type">对话框类型和按钮组合</param>
        /// <returns>用户点击的按钮ID</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int MessageBox(nint hWnd, string text, string caption, uint type);
    }
}
