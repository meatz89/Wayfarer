/// <summary>
/// Generates dialogue from categorical templates - NO hardcoded text
/// All narrative must come from templates and categorical properties
/// DDR-007: All dialogue selection is deterministic based on game state
/// </summary>
public class DialogueGenerationService
{
    private DialogueTemplates _templates;

    public DialogueGenerationService(GameWorld gameWorld)
    {
        _templates = gameWorld.DialogueTemplates;
        if (_templates.ConnectionStateDialogue == null)
        { }
        else
        { }
    }

    /// <summary>
    /// Generate NPC dialogue from connection state and context
    /// Returns categorical template, NOT English text
    /// DDR-007: Deterministic selection based on turn number (predictable)
    /// </summary>
    public string GenerateNPCDialogue(
        ConnectionState state,
        PersonalityType personality,
        MeetingObligation meeting,
        int turnNumber)
    {
        string stateKey = state.ToString();

        if (!_templates.ConnectionStateDialogue.ContainsKey(stateKey))
            return "emotional:neutral query:general";

        ConnectionStateTemplate stateTemplate = _templates.ConnectionStateDialogue[stateKey];

        // Use personality-based dialogue
        if (stateTemplate.Personality.ContainsKey(personality.ToString()))
        {
            List<string> options = stateTemplate.Personality[personality.ToString()];
            // DDR-007: Deterministic selection based on turn number
            int deterministicIndex = turnNumber % options.Count;
            return options[deterministicIndex];
        }

        // Fall back to default for state
        if (stateTemplate.Default.Any())
        {
            // DDR-007: Deterministic selection based on turn number
            int deterministicIndex = turnNumber % stateTemplate.Default.Count;
            return stateTemplate.Default[deterministicIndex];
        }

        return "emotional:neutral query:general";
    }

    /// <summary>
    /// Generate card dialogue from template type
    /// Returns categorical template, NOT English text
    /// DDR-007: Deterministic selection based on template ID hash
    /// </summary>
    public string GenerateCardDialogue(string templateId, bool isPlayer)
    {
        string key = templateId;

        if (_templates.CardDialogue.Categories.ContainsKey(key))
        {
            CardCategoryDialogue category = _templates.CardDialogue.Categories[key];
            List<string> options = isPlayer ? category.Player : category.Npc;

            if (options.Any())
            {
                // DDR-007: Deterministic selection based on template ID
                int deterministicIndex = Math.Abs(templateId.GetHashCode()) % options.Count;
                return options[deterministicIndex];
            }
        }

        return isPlayer ? "action:generic" : "response:generic";
    }

    /// <summary>
    /// Generate NPC description from profession and state
    /// Returns categorical template, NOT English text
    /// DDR-007: Deterministic selection based on NPC name hash
    /// </summary>
    public string GenerateNPCDescription(NPC npc, ConnectionState state)
    {
        List<string> elements = new List<string>();

        // Add profession base
        string profKey = npc.Profession.ToString();
        if (_templates.NpcDescriptions?.ProfessionBase?.ContainsKey(profKey) == true)
        {
            List<string> profOptions = _templates.NpcDescriptions.ProfessionBase[profKey];
            if (profOptions.Any())
            {
                // DDR-007: Deterministic selection based on NPC name
                int deterministicIndex = Math.Abs(npc.Name.GetHashCode()) % profOptions.Count;
                elements.Add(profOptions[deterministicIndex]);
            }
        }

        // Add emotional modifiers
        string stateKey = state.ToString();
        if (_templates.NpcDescriptions?.EmotionalModifiers?.ContainsKey(stateKey) == true)
        {
            Dictionary<string, List<string>> modifiers = _templates.NpcDescriptions.EmotionalModifiers[stateKey];
        }

        return elements.Any() ? string.Join(" ", elements) : "focus:neutral activity:general";
    }
}