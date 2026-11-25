using System.Text.Json.Serialization;

namespace SolidApi.Models;

public class ApplicationResource
{
    [JsonPropertyName("res-type")]      public string ResType { get; set; } = "application";
    [JsonPropertyName("resource-name")] public string ResourceName { get; set; } = "";
    [JsonPropertyName("resource-desc")] public string? ResourceDesc { get; set; }
    [JsonPropertyName("dt-creation")]   public string DtCreation { get; set; } = "";
}