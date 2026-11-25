using System.Text.Json.Serialization;

namespace SolidApi.Models;

public class ResourceRequest
{
    [JsonPropertyName("res-type")]      public string? ResType { get; set; }
    [JsonPropertyName("resource-name")] public string? ResourceName { get; set; }
    [JsonPropertyName("resource-desc")] public string? ResourceDesc { get; set; }
    [JsonPropertyName("content-type")]  public string? ContentType { get; set; }
    [JsonPropertyName("content")]       public string? Content { get; set; }
    [JsonPropertyName("evt")]           public int? Evt { get; set; }
    [JsonPropertyName("endpoint")]      public string? Endpoint { get; set; }
    [JsonPropertyName("dt-expiration")] public string? DtExpiration { get; set; }
}