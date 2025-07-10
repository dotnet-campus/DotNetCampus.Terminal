using System.Text.Json.Serialization;
using DotNetCampus.Terminal.Modules.Configurations.Models;

namespace DotNetCampus.Terminal.Modules.Configurations.JsonSource;

/// <summary>
/// JSON序列化上下文，用于AOT编译支持
/// </summary>
[JsonSerializable(typeof(DeviceConfiguration))]
[JsonSerializable(typeof(SshRemoteDeviceInfo))]
[JsonSerializable(typeof(SyncGroupConfiguration))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    GenerationMode = JsonSourceGenerationMode.Default,
    UseStringEnumConverter = true
)]
public partial class ConfigurationJsonContext : JsonSerializerContext
{
}
