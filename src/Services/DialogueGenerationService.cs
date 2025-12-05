/// <summary>
/// Generates dialogue from categorical templates - NO hardcoded text
/// All narrative must come from templates and categorical properties
/// DDR-007: All dialogue selection is deterministic based on game state
/// STATELESS SERVICE: Reads templates from GameWorld on demand (no cached fields)
/// </summary>
public class DialogueGenerationService
{
    private readonly GameWorld _gameWorld;

    public DialogueGenerationService(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
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
        DialogueTemplates templates = _gameWorld.DialogueTemplates;
        string stateKey = state.ToString();

        if (!templates.ConnectionStateDialogue.ContainsKey(stateKey))
            return "emotional:neutral query:general";

        ConnectionStateTemplate stateTemplate = templates.ConnectionStateDialogue[stateKey];

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
        DialogueTemplates templates = _gameWorld.DialogueTemplates;
        string key = templateId;

        if (templates.CardDialogue.Categories.ContainsKey(key))
        {
            CardCategoryDialogue category = templates.CardDialogue.Categories[key];
            List<string> options = isPlayer ? category.Player : category.Npc;

            if (options.Any())
            {
                // DDR-007: Use first option (categorical selection, no hash-based pseudo-randomness)
                return options[0];
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
        DialogueTemplates templates = _gameWorld.DialogueTemplates;
        List<string> elements = new List<string>();

        // Add profession base
        string profKey = npc.Profession.ToString();
        if (templates.NpcDescriptions?.ProfessionBase?.ContainsKey(profKey) == true)
        {
            List<string> profOptions = templates.NpcDescriptions.ProfessionBase[profKey];
            if (profOptions.Any())
            {
                // DDR-007: Use first option (categorical selection, no hash-based pseudo-randomness)
                elements.Add(profOptions[0]);
            }
        }

        return elements.Any() ? string.Join(" ", elements) : "focus:neutral activity:general";
    }
}
