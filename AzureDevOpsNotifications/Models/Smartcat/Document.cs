using System.Text.Json.Serialization;

namespace AzureDevOpsNotifications.Models.Smartcat;

public class Document
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("fullPath")]
    public string FullPath { get; set; }

    [JsonPropertyName("creationDate")]
    public DateTime CreationDate { get; set; }

    [JsonPropertyName("sourceLanguage")]
    public string SourceLanguage { get; set; }

    [JsonPropertyName("documentDisassemblingStatus")]
    public string DocumentDisassemblingStatus { get; set; }

    [JsonPropertyName("targetLanguage")]
    public string TargetLanguage { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("wordsCount")]
    public int WordsCount { get; set; }

    [JsonPropertyName("statusModificationDate")]
    public DateTime StatusModificationDate { get; set; }

    [JsonPropertyName("pretranslateCompleted")]
    public bool PretranslateCompleted { get; set; }

    [JsonPropertyName("workflowStages")]
    public List<WorkflowStage> WorkflowStages { get; set; }

    [JsonPropertyName("externalId")]
    public string ExternalId { get; set; }

    [JsonPropertyName("placeholdersAreEnabled")]
    public bool PlaceholdersAreEnabled { get; set; }
}