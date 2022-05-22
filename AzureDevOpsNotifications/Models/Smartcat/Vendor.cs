using System.Text.Json.Serialization;

namespace AzureDevOpsNotifications.Models.Smartcat;

public class Vendor
{
    [JsonPropertyName("vendorAccountId")]
    public string VendorAccountId { get; set; }

    [JsonPropertyName("removedFromProject")]
    public bool RemovedFromProject { get; set; }

    [JsonPropertyName("cost")]
    public Cost Cost { get; set; }

    [JsonPropertyName("costDetailsFileId")]
    public string CostDetailsFileId { get; set; }
}