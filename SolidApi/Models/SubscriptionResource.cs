using System.Text.Json.Serialization;
namespace SolidApi.Models;

public class SubscriptionResource
{
    [JsonPropertyName("res-type")]      public string ResType { get; set; } = "subscription";
    [JsonPropertyName("resource-name")] public string ResourceName { get; set; } = "";
    [JsonPropertyName("evt")] public int Evt { get; set; } //1 = criação, 2 = eliminação
    [JsonPropertyName("endpoint")] public string? Endpoint { get; set; }
    [JsonPropertyName("dt-creation")]   public string DtCreation { get; set; } = "";
    [JsonPropertyName("dt-expiration")] public string DtExpiration { get; set; } = "";
}
