
using System.Text.Json;
using System.Text.RegularExpressions;

public class EncounterSummaryParser
{
    // Define section names that match the expected output from the AI
    private static readonly string[] SectionNames = new[]
    {
        "ENCOUNTER OUTCOME",
        "INVENTORY CHANGES",
        "RESOURCE CHANGES",
        "DISCOVERED LOCATIONS",
        "NEW NPCS",
        "RELATIONSHIP CHANGES",
        "QUESTS",
        "JOBS",
        "RUMORS",
        "TIME PASSAGE"
    };

    // Options for JSON deserialization
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public class EncounterSummaryResult
    {
        public EncounterOutcome OutcomeResult { get; set; } = new EncounterOutcome();
        public List<InventoryItem> InventoryChanges { get; set; } = new List<InventoryItem>();
        public ResourceChange ResourceChanges { get; set; } = new ResourceChange();
        public List<DiscoveredLocation> DiscoveredLocations { get; set; } = new List<DiscoveredLocation>();
        public List<NPC> NewNPCs { get; set; } = new List<NPC>();
        public List<RelationshipChange> RelationshipChanges { get; set; } = new List<RelationshipChange>();
        public List<Quest> Quests { get; set; } = new List<Quest>();
        public List<Job> Jobs { get; set; } = new List<Job>();
        public List<Rumor> Rumors { get; set; } = new List<Rumor>();
        public TimePassage TimePassage { get; set; } = new TimePassage();

        // Track parsing errors by section
        public Dictionary<string, string> ParseErrors { get; set; } = new Dictionary<string, string>();

        // Track which sections were successfully parsed
        public HashSet<string> SuccessfullyParsedSections { get; set; } = new HashSet<string>();
    }

    public EncounterSummaryResult ParseSummary(string aiResponse)
    {
        var result = new EncounterSummaryResult();

        // Extract each section
        var sections = ExtractSections(aiResponse);

        // Process each section independently
        foreach (var sectionName in SectionNames)
        {
            // Skip if section is missing
            if (!sections.ContainsKey(sectionName))
            {
                result.ParseErrors[sectionName] = "Section not found in AI response";
                continue;
            }

            string jsonContent = sections[sectionName];

            try
            {
                // Try to parse each section based on its name
                switch (sectionName)
                {
                    case "ENCOUNTER OUTCOME":
                        result.OutcomeResult = JsonSerializer.Deserialize<EncounterOutcome>(jsonContent, Options) ?? new EncounterOutcome();
                        result.SuccessfullyParsedSections.Add(sectionName);
                        break;

                    case "INVENTORY CHANGES":
                        result.InventoryChanges = JsonSerializer.Deserialize<List<InventoryItem>>(jsonContent, Options) ?? new List<InventoryItem>();
                        result.SuccessfullyParsedSections.Add(sectionName);
                        break;

                    case "RESOURCE CHANGES":
                        result.ResourceChanges = JsonSerializer.Deserialize<ResourceChange>(jsonContent, Options) ?? new ResourceChange();
                        result.SuccessfullyParsedSections.Add(sectionName);
                        break;

                    case "DISCOVERED LOCATIONS":
                        result.DiscoveredLocations = JsonSerializer.Deserialize<List<DiscoveredLocation>>(jsonContent, Options) ?? new List<DiscoveredLocation>();
                        result.SuccessfullyParsedSections.Add(sectionName);
                        break;

                    case "NEW NPCS":
                        result.NewNPCs = JsonSerializer.Deserialize<List<NPC>>(jsonContent, Options) ?? new List<NPC>();
                        result.SuccessfullyParsedSections.Add(sectionName);
                        break;

                    case "RELATIONSHIP CHANGES":
                        result.RelationshipChanges = JsonSerializer.Deserialize<List<RelationshipChange>>(jsonContent, Options) ?? new List<RelationshipChange>();
                        result.SuccessfullyParsedSections.Add(sectionName);
                        break;

                    case "QUESTS":
                        result.Quests = JsonSerializer.Deserialize<List<Quest>>(jsonContent, Options) ?? new List<Quest>();
                        result.SuccessfullyParsedSections.Add(sectionName);
                        break;

                    case "JOBS":
                        result.Jobs = JsonSerializer.Deserialize<List<Job>>(jsonContent, Options) ?? new List<Job>();
                        result.SuccessfullyParsedSections.Add(sectionName);
                        break;

                    case "RUMORS":
                        result.Rumors = JsonSerializer.Deserialize<List<Rumor>>(jsonContent, Options) ?? new List<Rumor>();
                        result.SuccessfullyParsedSections.Add(sectionName);
                        break;

                    case "TIME PASSAGE":
                        result.TimePassage = JsonSerializer.Deserialize<TimePassage>(jsonContent, Options) ?? new TimePassage();
                        result.SuccessfullyParsedSections.Add(sectionName);
                        break;
                }
            }
            catch (JsonException ex)
            {
                // Try to fix common JSON issues and retry
                try
                {
                    string fixedJson = AttemptToFixJson(jsonContent);
                    if (fixedJson != jsonContent) // Only retry if the fix changed something
                    {
                        switch (sectionName)
                        {
                            case "ENCOUNTER OUTCOME":
                                result.OutcomeResult = JsonSerializer.Deserialize<EncounterOutcome>(fixedJson, Options) ?? new EncounterOutcome();
                                result.SuccessfullyParsedSections.Add(sectionName);
                                break;

                            case "INVENTORY CHANGES":
                                result.InventoryChanges = JsonSerializer.Deserialize<List<InventoryItem>>(fixedJson, Options) ?? new List<InventoryItem>();
                                result.SuccessfullyParsedSections.Add(sectionName);
                                break;

                            case "RESOURCE CHANGES":
                                result.ResourceChanges = JsonSerializer.Deserialize<ResourceChange>(fixedJson, Options) ?? new ResourceChange();
                                result.SuccessfullyParsedSections.Add(sectionName);
                                break;

                            case "DISCOVERED LOCATIONS":
                                result.DiscoveredLocations = JsonSerializer.Deserialize<List<DiscoveredLocation>>(fixedJson, Options) ?? new List<DiscoveredLocation>();
                                result.SuccessfullyParsedSections.Add(sectionName);
                                break;

                            case "NEW NPCS":
                                result.NewNPCs = JsonSerializer.Deserialize<List<NPC>>(fixedJson, Options) ?? new List<NPC>();
                                result.SuccessfullyParsedSections.Add(sectionName);
                                break;

                            case "RELATIONSHIP CHANGES":
                                result.RelationshipChanges = JsonSerializer.Deserialize<List<RelationshipChange>>(fixedJson, Options) ?? new List<RelationshipChange>();
                                result.SuccessfullyParsedSections.Add(sectionName);
                                break;

                            case "QUESTS":
                                result.Quests = JsonSerializer.Deserialize<List<Quest>>(fixedJson, Options) ?? new List<Quest>();
                                result.SuccessfullyParsedSections.Add(sectionName);
                                break;

                            case "JOBS":
                                result.Jobs = JsonSerializer.Deserialize<List<Job>>(fixedJson, Options) ?? new List<Job>();
                                result.SuccessfullyParsedSections.Add(sectionName);
                                break;

                            case "RUMORS":
                                result.Rumors = JsonSerializer.Deserialize<List<Rumor>>(fixedJson, Options) ?? new List<Rumor>();
                                result.SuccessfullyParsedSections.Add(sectionName);
                                break;

                            case "TIME PASSAGE":
                                result.TimePassage = JsonSerializer.Deserialize<TimePassage>(fixedJson, Options) ?? new TimePassage();
                                result.SuccessfullyParsedSections.Add(sectionName);
                                break;
                        }
                    }
                    else
                    {
                        // If fix didn't change anything, log original error
                        result.ParseErrors[sectionName] = $"JSON Error: {ex.Message}";
                    }
                }
                catch (Exception retryEx)
                {
                    // Log both original and retry errors
                    result.ParseErrors[sectionName] = $"JSON Error: {ex.Message}. Retry failed: {retryEx.Message}";
                }
            }
            catch (Exception ex)
            {
                // Log the error but continue processing other sections
                result.ParseErrors[sectionName] = $"Error parsing: {ex.Message}";
            }
        }

        return result;
    }

    private Dictionary<string, string> ExtractSections(string aiResponse)
    {
        var sections = new Dictionary<string, string>();

        // This pattern matches sections like: ### SECTION NAME\n```json\n[content]\n```
        string pattern = @"###\s+([A-Z\s]+)\s*\n```json\s*\n([\s\S]*?)\n```";
        var matches = Regex.Matches(aiResponse, pattern);

        foreach (Match match in matches)
        {
            if (match.Groups.Count >= 3)
            {
                string sectionName = match.Groups[1].Value.Trim();
                string jsonContent = match.Groups[2].Value.Trim();

                // Clean up common JSON formatting issues
                jsonContent = CleanupJsonContent(jsonContent);

                sections[sectionName] = jsonContent;
            }
        }

        // If we didn't find any matches with the expected format, try a looser pattern
        if (sections.Count == 0)
        {
            // Try to find any JSON blocks preceded by a title-like line
            pattern = @"(?:#{1,3}\s*|^)([A-Z][A-Z\s]+)(?:\s*:\s*|\n)(?:```(?:json)?\s*\n)?([\s\S]*?)(?:\n```|(?=\n(?:#{1,3}|[A-Z][A-Z\s]+)))";
            matches = Regex.Matches(aiResponse, pattern);

            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 3)
                {
                    string possibleSectionName = match.Groups[1].Value.Trim().ToUpper();
                    string jsonContent = match.Groups[2].Value.Trim();

                    // Find the closest matching section name
                    string closestSectionName = SectionNames.FirstOrDefault(s =>
                        s.Contains(possibleSectionName) || possibleSectionName.Contains(s));

                    if (!string.IsNullOrEmpty(closestSectionName) && !sections.ContainsKey(closestSectionName))
                    {
                        // Clean up and add the content
                        jsonContent = CleanupJsonContent(jsonContent);
                        sections[closestSectionName] = jsonContent;
                    }
                }
            }
        }

        return sections;
    }

    private string CleanupJsonContent(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return "{}";

        // Remove trailing commas in arrays and objects (common AI mistake)
        json = Regex.Replace(json, @",(\s*[\]}])", "$1");

        // Fix boolean values that might be capitalized
        json = Regex.Replace(json, @"\bTrue\b", "true", RegexOptions.IgnoreCase);
        json = Regex.Replace(json, @"\bFalse\b", "false", RegexOptions.IgnoreCase);

        // Remove comments (LLMs sometimes add these)
        json = Regex.Replace(json, @"//.*?$", "", RegexOptions.Multiline);
        json = Regex.Replace(json, @"/\*[\s\S]*?\*/", "");

        // Ensure proper quotes for property names (replace single quotes with double quotes)
        json = Regex.Replace(json, @"([{,]\s*)'([^']+)'(\s*:)", "$1\"$2\"$3");

        return json;
    }

    private string AttemptToFixJson(string json)
    {
        // Handle case where single object is given when array expected
        if (!json.StartsWith("[") && (
            json.Contains("\"item\"") || // inventory item
            json.Contains("\"name\"") || // location/NPC
            json.Contains("\"npc\"") || // relationship
            json.Contains("\"title\"") || // quest/job
            json.Contains("\"content\"") // rumor
        ))
        {
            // Wrap in array brackets if it looks like a single object of array type
            if (json.StartsWith("{") && json.EndsWith("}"))
            {
                return "[" + json + "]";
            }
        }

        // Add missing closing brackets/braces
        int openBraces = json.Count(c => c == '{');
        int closeBraces = json.Count(c => c == '}');
        int openBrackets = json.Count(c => c == '[');
        int closeBrackets = json.Count(c => c == ']');

        string fixedJson = json;

        // Add missing closing braces
        for (int i = 0; i < openBraces - closeBraces; i++)
        {
            fixedJson += "}";
        }

        // Add missing closing brackets
        for (int i = 0; i < openBrackets - closeBrackets; i++)
        {
            fixedJson += "]";
        }

        // Fix missing quotes around property names
        fixedJson = Regex.Replace(fixedJson, @"([{,]\s*)([a-zA-Z0-9_]+)(\s*:)", "$1\"$2\"$3");

        // Fix missing comma between array elements
        fixedJson = Regex.Replace(fixedJson, @"}(\s*){", "},\n$1{");

        return fixedJson;
    }
}
