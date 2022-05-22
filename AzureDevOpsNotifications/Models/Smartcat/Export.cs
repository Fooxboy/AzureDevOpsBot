using System.Text.Json.Serialization;

namespace AzureDevOpsNotifications.Models.Smartcat;

public class Export
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("documentIds")]
    public List<string> DocumentIds { get; set; }
}