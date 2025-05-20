using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cogwheel;
using CommunityToolkit.Mvvm.ComponentModel;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Framework;
using Container = YoutubeExplode.Videos.Streams.Container;

namespace YoutubeDownloader.Services;

/// <summary>
/// 应用程序设置服务，负责管理和持久化用户配置
/// 使用ObservableObject特性自动实现INotifyPropertyChanged接口
/// </summary>
[ObservableObject]
public partial class SettingsService()
    : SettingsBase(
        Path.Combine(AppContext.BaseDirectory, "Settings.dat"),
        SerializerContext.Default
    )
{
    /// <summary>
    /// 是否启用乌克兰支持消息
    /// </summary>
    [ObservableProperty]
    public partial bool IsUkraineSupportMessageEnabled { get; set; } = true;

    /// <summary>
    /// 应用程序主题设置
    /// </summary>
    [ObservableProperty]
    public partial ThemeVariant Theme { get; set; }

    /// <summary>
    /// 是否启用自动更新功能
    /// </summary>
    [ObservableProperty]
    public partial bool IsAutoUpdateEnabled { get; set; } = true;

    /// <summary>
    /// 是否持久化保存认证信息
    /// </summary>
    [ObservableProperty]
    public partial bool IsAuthPersisted { get; set; } = true;

    /// <summary>
    /// 是否在下载时注入特定语言的音频流
    /// </summary>
    [ObservableProperty]
    public partial bool ShouldInjectLanguageSpecificAudioStreams { get; set; } = true;

    /// <summary>
    /// 是否在下载时注入字幕
    /// </summary>
    [ObservableProperty]
    public partial bool ShouldInjectSubtitles { get; set; } = true;

    /// <summary>
    /// 是否在下载时注入媒体标签
    /// </summary>
    [ObservableProperty]
    public partial bool ShouldInjectTags { get; set; } = true;

    /// <summary>
    /// 是否跳过已存在的文件
    /// </summary>
    [ObservableProperty]
    public partial bool ShouldSkipExistingFiles { get; set; }

    /// <summary>
    /// 下载文件的命名模板
    /// </summary>
    [ObservableProperty]
    public partial string FileNameTemplate { get; set; } = "$title";

    /// <summary>
    /// 并行下载的最大数量
    /// </summary>
    [ObservableProperty]
    public partial int ParallelLimit { get; set; } = 2;

    /// <summary>
    /// 上次使用的认证Cookie
    /// </summary>
    [ObservableProperty]
    public partial IReadOnlyList<Cookie>? LastAuthCookies { get; set; }

    /// <summary>
    /// 上次使用的视频容器格式
    /// </summary>
    [ObservableProperty]
    [JsonConverter(typeof(ContainerJsonConverter))]
    public partial Container LastContainer { get; set; } = Container.Mp4;

    /// <summary>
    /// 上次使用的视频质量偏好设置
    /// </summary>
    [ObservableProperty]
    public partial VideoQualityPreference LastVideoQualityPreference { get; set; } =
        VideoQualityPreference.Highest;

    /// <summary>
    /// 保存设置到文件
    /// </summary>
    /// <remarks>
    /// 如果用户选择不持久化认证信息，则在保存前临时清除Cookie
    /// </remarks>
    public override void Save()
    {
        // 清除Cookie如果不需要持久化
        var lastAuthCookies = LastAuthCookies;
        if (!IsAuthPersisted)
            LastAuthCookies = null;

        base.Save();

        // 恢复Cookie以便继续使用
        LastAuthCookies = lastAuthCookies;
    }
}

/// <summary>
/// SettingsService的部分类实现，包含Container类型的JSON转换器
/// </summary>
public partial class SettingsService
{
    /// <summary>
    /// 用于Container类型的JSON序列化和反序列化的自定义转换器
    /// </summary>
    private class ContainerJsonConverter : JsonConverter<Container>
    {
        /// <summary>
        /// 从JSON读取Container对象
        /// </summary>
        /// <param name="reader">UTF8JSON读取器</param>
        /// <param name="typeToConvert">要转换的类型</param>
        /// <param name="options">JSON序列化选项</param>
        /// <returns>反序列化的Container对象</returns>
        public override Container Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            Container? result = null;

            if (reader.TokenType != JsonTokenType.StartObject)
                return result
                    ?? throw new InvalidOperationException(
                        $"Invalid JSON for type '{typeToConvert.FullName}'."
                    );
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (
                    reader.TokenType != JsonTokenType.PropertyName
                    || reader.GetString() != "Name"
                    || !reader.Read()
                    || reader.TokenType != JsonTokenType.String
                )
                    continue;
                var name = reader.GetString();
                if (!string.IsNullOrWhiteSpace(name))
                    result = new Container(name);
            }

            return result
                ?? throw new InvalidOperationException(
                    $"Invalid JSON for type '{typeToConvert.FullName}'."
                );
        }

        /// <summary>
        /// 将Container对象写入JSON
        /// </summary>
        /// <param name="writer">UTF8JSON写入器</param>
        /// <param name="value">要序列化的Container对象</param>
        /// <param name="options">JSON序列化选项</param>
        public override void Write(
            Utf8JsonWriter writer,
            Container value,
            JsonSerializerOptions options
        )
        {
            writer.WriteStartObject();
            writer.WriteString("Name", value.Name);
            writer.WriteEndObject();
        }
    }
}

/// <summary>
/// SettingsService的部分类实现，包含JSON序列化上下文
/// </summary>
public partial class SettingsService
{
    /// <summary>
    /// 用于SettingsService类型的JSON序列化上下文
    /// 使用源生成的JSON序列化器提高性能
    /// </summary>
    [JsonSerializable(typeof(SettingsService))]
    private partial class SerializerContext : JsonSerializerContext;
}
