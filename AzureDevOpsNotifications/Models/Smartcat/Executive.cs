using System.Text.Json.Serialization;

namespace AzureDevOpsNotifications.Models.Smartcat;

public class Executive
{
    [JsonPropertyName("assignedWordsCount")]
    public int AssignedWordsCount { get; set; }

    [JsonPropertyName("progress")]
    public int Progress { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("supplierType")]
    public string SupplierType { get; set; }
}