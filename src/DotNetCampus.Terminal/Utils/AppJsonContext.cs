using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetCampus.Terminal.Modules.Configurations.JsonSource;

namespace DotNetCampus.Terminal.Utils;

[JsonSerializable(typeof(DeviceConfiguration))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DictionaryKeyPolicy = JsonKnownNamingPolicy.Unspecified,
    ReadCommentHandling = JsonCommentHandling.Skip,
    UseStringEnumConverter = true,
    AllowOutOfOrderMetadataProperties = true,
    WriteIndented = true,
    NewLine = "\n")]
internal partial class AppJsonContext : JsonSerializerContext;
