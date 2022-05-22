using System.Text.Json.Serialization;

namespace AzureDevOpsNotifications.Models;

public class TranslateProject
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("smartcat")]
    public string Smartcat { get; set; }
    
    [JsonPropertyName("languages")]
    public List<string> Languages { get; set; }
    
    [JsonPropertyName("remote")]
    public string Remote { get; set; }
    
    [JsonPropertyName("token")]
    public string Token { get; set; }
}