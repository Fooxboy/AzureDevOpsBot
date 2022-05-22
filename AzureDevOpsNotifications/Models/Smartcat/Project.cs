using System.Text.Json.Serialization;

namespace AzureDevOpsNotifications.Models.Smartcat;

public class Project
{
     [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("accountId")]
        public string AccountId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("deadline")]
        public DateTime Deadline { get; set; }

        [JsonPropertyName("creationDate")]
        public DateTime CreationDate { get; set; }

        [JsonPropertyName("createdByUserId")]
        public string CreatedByUserId { get; set; }

        [JsonPropertyName("createdByUserEmail")]
        public string CreatedByUserEmail { get; set; }

        [JsonPropertyName("modificationDate")]
        public DateTime ModificationDate { get; set; }

        [JsonPropertyName("sourceLanguageId")]
        public int SourceLanguageId { get; set; }

        [JsonPropertyName("sourceLanguage")]
        public string SourceLanguage { get; set; }

        [JsonPropertyName("targetLanguages")]
        public List<string> TargetLanguages { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("statusModificationDate")]
        public DateTime StatusModificationDate { get; set; }

        [JsonPropertyName("domainId")]
        public int DomainId { get; set; }

        [JsonPropertyName("clientId")]
        public string ClientId { get; set; }

        [JsonPropertyName("vendors")]
        public List<Vendor> Vendors { get; set; }

        [JsonPropertyName("workflowStages")]
        public List<WorkflowStage> WorkflowStages { get; set; }

        [JsonPropertyName("documents")]
        public List<Document> Documents { get; set; }

        [JsonPropertyName("externalTag")]
        public string ExternalTag { get; set; }

        [JsonPropertyName("specializations")]
        public List<string> Specializations { get; set; }

        [JsonPropertyName("managers")]
        public List<string> Managers { get; set; }

        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonPropertyName("customFields")]
        public object CustomFields { get; set; }
}