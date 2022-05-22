using System.Text.Json.Serialization;

namespace AzureDevOpsNotifications.Models.Smartcat;

public class WorkflowStage
{
    [JsonPropertyName("progress")]
    public double Progress { get; set; }

    [JsonPropertyName("stageType")]
    public string StageType { get; set; }

    [JsonPropertyName("wordsTranslated")]
    public int WordsTranslated { get; set; }

    [JsonPropertyName("unassignedWordsCount")]
    public int UnassignedWordsCount { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("executives")]
    public List<object> Executives { get; set; }
}