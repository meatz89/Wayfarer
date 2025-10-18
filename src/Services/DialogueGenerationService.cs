using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Generates dialogue from categorical templates - NO hardcoded text
/// All narrative must come from templates and categorical properties
/// </summary>
public class DialogueGenerationService
{
    private DialogueTemplates _templates;
    private readonly Random _random = new Random();

    public DialogueGenerationService(GameWorld gameWorld)
    {
        _templates = gameWorld.DialogueTemplates ?? new DialogueTemplates();
        if (_templates.ConnectionStateDialogue == null)
        {}
        else
        {}
    }

    /// <summary>
    /// Generate NPC dialogue from connection state and context
    /// Returns categorical template, NOT English text
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
        if (stateTemplate.Personality != null &&
            stateTemplate.Personality.ContainsKey(personality.ToString()))
        {
            List<string> options = stateTemplate.Personality[personality.ToString()];
            return options[_random.Next(options.Count)];
        }

        // Fall back to default for state
        if (stateTemplate.Default != null && stateTemplate.Default.Any())
        {
            return stateTemplate.Default[_random.Next(stateTemplate.Default.Count)];
        }

        return "emotional:neutral query:general";
    }

    /// <summary>
    /// Generate card dialogue from template type
    /// Returns categorical template, NOT English text
    /// </summary>
    public string GenerateCardDialogue(string templateId, bool isPlayer)
    {
        string key = templateId;

        if (_templates.CardDialogue?.Categories?.ContainsKey(key) == true)
        {
            CardCategoryDialogue category = _templates.CardDialogue.Categories[key];
            List<string> options = isPlayer ? category.Player : category.Npc;

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
                elements.Add(profOptions[_random.Next(profOptions.Count)]);
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

    /// <summary>
    /// Generate atmosphere description from location properties
    /// Returns categorical template, NOT English text
    /// </summary>
    public string GenerateAtmosphereDescription(
        List<LocationPropertyType> properties,
        TimeBlocks timeBlock,
        int urgentObligations,
        int npcsPresent)
    {
        List<string> elements = new List<string>();

        // Add time element
        elements.Add($"time:{timeBlock.ToString().ToLower()}");

        // Add property-based elements
        foreach (LocationPropertyType prop in properties)
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