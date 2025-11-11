using System.Text.Json;

/// <summary>
/// Repository that loads and manages JSON narrative content for conversation generation.
/// Provides template matching and card narrative retrieval based on conversation state.
/// </summary>
public class JsonNarrativeRepository
{
    private readonly IContentDirectory contentDirectory;
    private List<NarrativeTemplate> templates = new List<NarrativeTemplate>();
    private Dictionary<string, string> defaultCardResponses = new Dictionary<string, string>();
    private bool isLoaded = false;

    public JsonNarrativeRepository(IContentDirectory contentDirectory)
    {
        this.contentDirectory = contentDirectory;
    }

    /// <summary>
    /// Finds the best matching narrative template for the current conversation state.
    /// Uses NPC ID, flow range, and rapport range to determine the best fit.
    /// </summary>
    public NarrativeTemplate FindBestMatch(SocialChallengeState state, NPCData npcData)
    {
        if (!isLoaded)
        {
            string filePath = Path.Combine(contentDirectory.Path, "Narratives", "conversation_narratives.json");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Narrative file not found at: {filePath}");
            }

            string jsonContent = File.ReadAllText(filePath);
            JsonDocument document = JsonDocument.Parse(jsonContent);
            JsonElement root = document.RootElement;

            // Load narrative templates
            if (root.TryGetProperty("narrativeTemplates", out JsonElement templatesElement))
            {
                foreach (JsonElement templateElement in templatesElement.EnumerateArray())
                {
                    NarrativeTemplate template = ParseNarrativeTemplate(templateElement);
                    templates.Add(template);
                }
            }

            // Load default responses
            if (root.TryGetProperty("defaultResponses", out JsonElement defaultElement) &&
                defaultElement.TryGetProperty("cardCategories", out JsonElement categoriesElement))
            {
                foreach (JsonProperty category in categoriesElement.EnumerateObject())
                {
                    defaultCardResponses[category.Name] = category.Value.GetString();
                }
            }

            isLoaded = true;
        }

        // First, try to find exact NPC matches
        List<NarrativeTemplate> npcMatches = templates
            .Where(t => t.Conditions.NpcId == npcData.NpcId)
            .ToList();

        NarrativeTemplate bestMatch = FindBestMatchFromCandidates(npcMatches, state);
        if (bestMatch != null)
        {
            return bestMatch;
        }

        // Fall back to generic templates (null npcId)
        List<NarrativeTemplate> genericMatches = templates
            .Where(t => t.Conditions.NpcId == null)
            .ToList();

        bestMatch = FindBestMatchFromCandidates(genericMatches, state);
        if (bestMatch != null)
        {
            return bestMatch;
        }

        // Final fallback - return first generic template
        return templates.FirstOrDefault(t => t.Conditions.NpcId == null);
    }

    /// <summary>
    /// Gets narrative text for a specific card, using category-based fallbacks.
    /// Checks template mappings first, then category defaults, then generic default.
    /// </summary>
    public string GetCardNarrative(string cardId, string category, NarrativeTemplate template)
    {
        // Check template-specific mappings first
        if (template?.CardMappings != null)
        {
            if (template.CardMappings.TryGetValue(cardId, out string specificMapping))
            {
                return specificMapping;
            }

            if (template.CardMappings.TryGetValue(category, out string categoryMapping))
            {
                return categoryMapping;
            }

            if (template.CardMappings.TryGetValue("default", out string defaultMapping))
            {
                return defaultMapping;
            }
        }

        // Fall back to global category defaults
        if (defaultCardResponses.TryGetValue(category, out string globalCategory))
        {
            return globalCategory;
        }

        // Final fallback
        return "You consider your response carefully.";
    }

    private NarrativeTemplate FindBestMatchFromCandidates(List<NarrativeTemplate> candidates, SocialChallengeState state)
    {
        if (!candidates.Any())
        {
            return null;
        }

        NarrativeTemplate bestMatch = null;
        int bestScore = -1;

        foreach (NarrativeTemplate template in candidates)
        {
            int score = CalculateMatchScore(template.Conditions, state);
            if (score > bestScore)
            {
                bestScore = score;
                bestMatch = template;
            }
        }

        return bestScore >= 0 ? bestMatch : null;
    }

    private int CalculateMatchScore(NarrativeConditions conditions, SocialChallengeState state)
    {
        int score = 0;

        // Flow range check
        bool flowMatch = state.Flow >= conditions.FlowMin && state.Flow <= conditions.FlowMax;
        if (!flowMatch)
        {
            return -1; // No match if flow is out of range
        }
        score += 10; // Base score for flow match

        // Rapport range check
        bool rapportMatch = state.Momentum >= conditions.RapportMin && state.Momentum <= conditions.RapportMax;
        if (!rapportMatch)
        {
            return -1; // No match if rapport is out of range
        }
        score += 10; // Base score for rapport match

        // Prefer narrower ranges (more specific templates)
        int flowRange = conditions.FlowMax - conditions.FlowMin;
        int rapportRange = conditions.RapportMax - conditions.RapportMin;
        score += Math.Max(0, 100 - flowRange); // Smaller ranges get higher scores
        score += Math.Max(0, 100 - rapportRange);

        return score;
    }

    private NarrativeTemplate ParseNarrativeTemplate(JsonElement element)
    {
        NarrativeTemplate template = new NarrativeTemplate
        {
            Id = element.GetProperty("id").GetString()
        };

        // Parse conditions
        JsonElement conditionsElement = element.GetProperty("conditions");
        template.Conditions = new NarrativeConditions
        {
            NpcId = conditionsElement.TryGetProperty("npcId", out JsonElement npcIdElement) && npcIdElement.ValueKind != JsonValueKind.Null
                ? npcIdElement.GetString()
                : null,
            FlowMin = conditionsElement.GetProperty("flowMin").GetInt32(),
            FlowMax = conditionsElement.GetProperty("flowMax").GetInt32(),
            RapportMin = conditionsElement.GetProperty("rapportMin").GetInt32(),
            RapportMax = conditionsElement.GetProperty("rapportMax").GetInt32()
        };

        // Parse dialogue and narrative text
        template.NPCDialogue = element.GetProperty("npcDialogue").GetString();
        if (element.TryGetProperty("narrativeText", out JsonElement narrativeElement))
        {
            template.NarrativeText = narrativeElement.GetString();
        }

        // Parse card mappings
        if (element.TryGetProperty("cardMappings", out JsonElement mappingsElement))
        {
            template.CardMappings = new Dictionary<string, string>();
            foreach (JsonProperty mapping in mappingsElement.EnumerateObject())
            {
                template.CardMappings[mapping.Name] = mapping.Value.GetString();
            }
        }

        return template;
    }
}

/// <summary>
/// Narrative template containing conditions and generated content.
/// </summary>
public class NarrativeTemplate
{
    public string Id { get; set; }
    public NarrativeConditions Conditions { get; set; }
    public string NPCDialogue { get; set; }
    public string NarrativeText { get; set; }
    public Dictionary<string, string> CardMappings { get; set; }
}

/// <summary>
/// Conditions that determine when a narrative template should be used.
/// </summary>
public class NarrativeConditions
{
    public string NpcId { get; set; } // null for generic templates
    public int FlowMin { get; set; }
    public int FlowMax { get; set; }
    public int RapportMin { get; set; }
    public int RapportMax { get; set; }
}