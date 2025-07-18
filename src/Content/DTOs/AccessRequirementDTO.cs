using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Wayfarer.Content.DTOs
{
    /// <summary>
    /// DTO for deserializing access requirements from JSON.
    /// </summary>
    public class AccessRequirementDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("requiredEquipment")]
        public List<string> RequiredEquipment { get; set; } = new List<string>();
        
        [JsonPropertyName("requiredItems")]
        public List<string> RequiredItemIds { get; set; } = new List<string>();
        
        [JsonPropertyName("requiredTokensPerNpc")]
        public Dictionary<string, int> RequiredTokensPerNPC { get; set; } = new Dictionary<string, int>();
        
        [JsonPropertyName("requiredTokensPerType")]
        public Dictionary<string, int> RequiredTokensPerType { get; set; } = new Dictionary<string, int>();
        
        [JsonPropertyName("logic")]
        public string Logic { get; set; } = "And";
        
        [JsonPropertyName("blockedMessage")]
        public string BlockedMessage { get; set; }
        
        [JsonPropertyName("hintMessage")]
        public string HintMessage { get; set; }
    }
}