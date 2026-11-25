using System.Text.Json.Serialization;

namespace SensorApp.Models;

// Representa um recurso do SOLID (application, channel ou data-record)
// com os nomes exatos das propriedades pedidos no enunciado.
public class ResourceRequest
{
    [JsonPropertyName("res-type")]      public string? ResType { get; set; }
    [JsonPropertyName("resource-name")] public string? ResourceName { get; set; }
    [JsonPropertyName("resource-desc")] public string? ResourceDesc { get; set; }
    [JsonPropertyName("content-type")]  public string? ContentType { get; set; }
    [JsonPropertyName("content")]       public string? Content { get; set; }
    [JsonPropertyName("dt-expiration")] public string? DtExpiration { get; set; }
}
