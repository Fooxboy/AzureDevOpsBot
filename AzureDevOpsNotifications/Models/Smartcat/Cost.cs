using System.Text.Json.Serialization;

namespace AzureDevOpsNotifications.Models.Smartcat;

public class Cost
{
    [JsonPropertyName("value")]
    public int Value { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("accuracyDegree")]
    public string AccuracyDegree { get; set; }

    [JsonPropertyName("detailsFileName")]
    public string DetailsFileName { get; set; }

    [JsonPropertyName("paymentStatus")]
    public string PaymentStatus { get; set; }
}