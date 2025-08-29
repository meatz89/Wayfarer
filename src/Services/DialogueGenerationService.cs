using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Generates dialogue from categorical templates - NO hardcoded text
/// All narrative must come from templates and categorical properties
/// </summary>
public class DialogueGenerationService
{
    private DialogueTemplates _templates;
    private readonly Random _random = new Random();
    
    public class DialogueTemplates
    {
        [JsonPropertyName("emotionalStateDialogue")]
        public Dictionary<string, EmotionalStateTemplate> EmotionalStateDialogue { get; set; }
        
        [JsonPropertyName("cardDialogue")]
        public CardDialogueTemplate CardDialogue { get; set; }
        
        [JsonPropertyName("npcDescriptions")]
        public NpcDescriptionTemplate NpcDescriptions { get; set; }
        
        [JsonPropertyName("narrativeElements")]
        public Dictionary<string, Dictionary<string, string>> NarrativeElements { get; set; }
    }
    
    public class EmotionalStateTemplate
    {
        [JsonPropertyName("contextual")]
        public Dictionary<string, object> Contextual { get; set; }
        
        [JsonPropertyName("personality")]
        public Dictionary<string, List<string>> Personality { get; set; }
        
        [JsonPropertyName("default")]
        public List<string> Default { get; set; }
    }
    
    public class CardDialogueTemplate
    {
        [JsonPropertyName("categories")]
        public Dictionary<string, CardCategoryDialogue> Categories { get; set; }
    }
    
    public class CardCategoryDialogue
    {
        [JsonPropertyName("player")]
        public List<string> Player { get; set; }
        
        [JsonPropertyName("npc")]
        public List<string> Npc { get; set; }
    }
    
    public class NpcDescriptionTemplate
    {
        [JsonPropertyName("professionBase")]
        public Dictionary<string, List<string>> ProfessionBase { get; set; }
        
        [JsonPropertyName("emotionalModifiers")]
        public Dictionary<string, Dictionary<string, List<string>>> EmotionalModifiers { get; set; }
    }
    
    public DialogueGenerationService(string contentPath)
    {
        LoadTemplates(Path.Combine(contentPath, "dialogue_templates.json"));
    }
    
    private void LoadTemplates(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Dialogue templates file not found: {path} - create the required JSON file");
        }
        
        var json = File.ReadAllText(path);
        _templates = JsonSerializer.Deserialize<DialogueTemplates>(json);
    }
    
    
    /// <summary>
    /// Generate NPC dialogue from emotional state and context
    /// Returns categorical template, NOT English text
    /// </summary>
    public string GenerateNPCDialogue(
        EmotionalState state, 
        PersonalityType personality,
        DeliveryObligation obligation = null,
        MeetingObligation meeting = null,
        int turnNumber = 0)
    {
        var stateKey = state.ToString();
        
        if (!_templates.EmotionalStateDialogue.ContainsKey(stateKey))
            return "emotional:neutral query:general";
            
        var stateTemplate = _templates.EmotionalStateDialogue[stateKey];
        
        // Check for contextual dialogue first (urgent letters, meetings)
        if (state == EmotionalState.DESPERATE)
        {
            if (meeting != null)
            {
                return GenerateMeetingDialogue(meeting, turnNumber);
            }
            if (obligation != null)
            {
                return GenerateObligationDialogue(obligation, turnNumber);
            }
        }
        
        // Use personality-based dialogue
        if (stateTemplate.Personality != null && 
            stateTemplate.Personality.ContainsKey(personality.ToString()))
        {
            var options = stateTemplate.Personality[personality.ToString()];
            return options[_random.Next(options.Count)];
        }
        
        // Fall back to default for state
        if (stateTemplate.Default != null && stateTemplate.Default.Any())
        {
            return stateTemplate.Default[_random.Next(stateTemplate.Default.Count)];
        }
        
        return "emotional:neutral query:general";
    }
    
    private string GenerateMeetingDialogue(MeetingObligation meeting, int turnNumber)
    {
        var hours = meeting.DeadlineInMinutes / 60;
        var urgency = hours <= 2 ? "critical" : hours <= 6 ? "urgent" : "pressing";
        
        return $"urgency:{urgency} time_pressure:{hours}h stakes:{meeting.Stakes.ToString().ToLower()} turn:{turnNumber}";
    }
    
    private string GenerateObligationDialogue(DeliveryObligation obligation, int turnNumber)
    {
        var hours = obligation.DeadlineInMinutes / 60;
        return $"letter_urgency:{hours}h recipient:{obligation.RecipientName} stakes:{obligation.Stakes.ToString().ToLower()} turn:{turnNumber}";
    }
    
    /// <summary>
    /// Generate card dialogue from template type
    /// Returns categorical template, NOT English text
    /// </summary>
    public string GenerateCardDialogue(string templateId, bool isPlayer)
    {
        var key = templateId;
        
        if (_templates.CardDialogue?.Categories?.ContainsKey(key) == true)
        {
            var category = _templates.CardDialogue.Categories[key];
            var options = isPlayer ? category.Player : category.Npc;
            
            if (options != null && options.Any())
            {
                return options[_random.Next(options.Count)];
            }
        }
        
        return isPlayer ? "action:generic" : "response:generic";
    }
    
    /// <summary>
    /// Generate NPC description from profession and state
    /// Returns categorical template, NOT English text
    /// </summary>
    public string GenerateNPCDescription(NPC npc, EmotionalState state, bool hasUrgentLetter = false)
    {
        var elements = new List<string>();
        
        // Add profession base
        var profKey = npc.Profession.ToString();
        if (_templates.NpcDescriptions?.ProfessionBase?.ContainsKey(profKey) == true)
        {
            var profOptions = _templates.NpcDescriptions.ProfessionBase[profKey];
            if (profOptions.Any())
            {
                elements.Add(profOptions[_random.Next(profOptions.Count)]);
            }
        }
        
        // Add emotional modifiers
        var stateKey = state.ToString();
        if (_templates.NpcDescriptions?.EmotionalModifiers?.ContainsKey(stateKey) == true)
        {
            var modifiers = _templates.NpcDescriptions.EmotionalModifiers[stateKey];
            var modKey = hasUrgentLetter && modifiers.ContainsKey("hasLetter") ? "hasLetter" : "default";
            
            if (modifiers.ContainsKey(modKey))
            {
                var modOptions = modifiers[modKey];
                if (modOptions.Any())
                {
                    elements.Add(modOptions[_random.Next(modOptions.Count)]);
                }
            }
        }
        
        return elements.Any() ? string.Join(" ", elements) : "presence:neutral activity:general";
    }
    
    /// <summary>
    /// Generate atmosphere description from spot properties
    /// Returns categorical template, NOT English text
    /// </summary>
    public string GenerateAtmosphereDescription(
        List<SpotPropertyType> properties,
        TimeBlocks timeBlock,
        int urgentObligations,
        int npcsPresent)
    {
        var elements = new List<string>();
        
        // Add time element
        elements.Add($"time:{timeBlock.ToString().ToLower()}");
        
        // Add property-based elements
        foreach (var prop in properties)
        {
            elements.Add($"property:{prop.ToString().ToLower()}");
        }
        
        // Add urgency if present
        if (urgentObligations > 0)
        {
            elements.Add($"urgency:obligations_{urgentObligations}");
        }
        
        // Add population
        elements.Add($"population:{(npcsPresent == 0 ? "empty" : npcsPresent < 3 ? "sparse" : "busy")}");
        
        return string.Join(" ", elements);
    }
}