using System.Text;

/// <summary>
/// Builds AI prompts for Scene/Situation narrative generation.
/// Creates structured prompts from ScenePromptContext containing entity objects and narrative hints.
///
/// ARCHITECTURE: Part of Two-Pass Procedural Generation (arc42 §8.28)
/// Pass 1 (Mechanical) produces complete Situation entities
/// Pass 2 (AI Narrative) uses this builder to create prompts for SceneNarrativeService
///
/// NO TEMPLATE FILES: Prompts built inline (simpler than file-based PromptBuilder in Social system)
/// </summary>
public class ScenePromptBuilder
{
    /// <summary>
    /// Build AI prompt for generating Situation narrative description.
    /// Includes entity context, narrative hints, and mechanical context.
    /// </summary>
    public string BuildSituationPrompt(ScenePromptContext context, NarrativeHints hints, Situation situation)
    {
        StringBuilder prompt = new StringBuilder();

        // System instruction
        prompt.AppendLine("You are a narrative writer for a Victorian-era social adventure game.");
        prompt.AppendLine("Generate ONE atmospheric sentence (50-120 characters). Strict limit.");
        prompt.AppendLine("Use a single vivid sensory detail. Pure atmosphere, no action.");
        prompt.AppendLine();

        // World context
        prompt.AppendLine("## World Context");
        prompt.AppendLine($"- Time: {FormatTimeBlock(context.CurrentTimeBlock)}");
        if (context.CurrentWeather != WeatherCondition.Clear)
            prompt.AppendLine($"- Weather: {FormatWeather(context.CurrentWeather)}");
        prompt.AppendLine($"- Day: {context.CurrentDay}");
        prompt.AppendLine();

        // Location context
        if (context.Location != null)
        {
            prompt.AppendLine("## Location");
            prompt.AppendLine($"- Name: {context.Location.Name}");
            prompt.AppendLine($"- Purpose: {context.Location.Purpose}");
            prompt.AppendLine($"- Privacy: {context.Location.Privacy}");
            prompt.AppendLine($"- Safety: {context.Location.Safety}");
            prompt.AppendLine($"- Activity: {context.Location.Activity}");
            if (!string.IsNullOrEmpty(context.Location.Description))
                prompt.AppendLine($"- Description: {context.Location.Description}");
            prompt.AppendLine();
        }

        // NPC context
        if (context.NPC != null)
        {
            prompt.AppendLine("## Character Present");
            prompt.AppendLine($"- Name: {context.NPC.Name}");
            prompt.AppendLine($"- Personality: {context.NPC.PersonalityType}");
            prompt.AppendLine($"- Profession: {context.NPC.Profession}");
            prompt.AppendLine($"- Current State: {context.NPC.CurrentState}");
            if (context.NPCBondLevel != 0)
                prompt.AppendLine($"- Relationship with player: Bond level {context.NPCBondLevel}");
            prompt.AppendLine();
        }

        // Route context (for travel situations)
        if (context.Route != null)
        {
            prompt.AppendLine("## Journey");
            prompt.AppendLine($"- Route: {context.Route.Name}");
            if (context.Route.OriginLocation != null)
                prompt.AppendLine($"- From: {context.Route.OriginLocation.Name}");
            if (context.Route.DestinationLocation != null)
                prompt.AppendLine($"- To: {context.Route.DestinationLocation.Name}");
            prompt.AppendLine();
        }

        // Narrative hints
        if (hints != null)
        {
            prompt.AppendLine("## Narrative Direction");
            if (!string.IsNullOrEmpty(hints.Tone))
                prompt.AppendLine($"- Tone: {hints.Tone}");
            if (!string.IsNullOrEmpty(hints.Theme))
                prompt.AppendLine($"- Theme: {hints.Theme}");
            if (!string.IsNullOrEmpty(hints.Context))
                prompt.AppendLine($"- Context: {hints.Context}");
            if (!string.IsNullOrEmpty(hints.Style))
                prompt.AppendLine($"- Style: {hints.Style}");
            prompt.AppendLine();
        }

        // Situation mechanical context
        if (situation != null)
        {
            prompt.AppendLine("## Situation");
            prompt.AppendLine($"- Type: {situation.Type}");
            prompt.AppendLine($"- Name: {situation.Name}");
            if (situation.Template?.ChoiceTemplates != null && situation.Template.ChoiceTemplates.Count > 0)
            {
                prompt.AppendLine($"- Available choices: {situation.Template.ChoiceTemplates.Count}");
                prompt.AppendLine("- Choice summaries:");
                foreach (ChoiceTemplate choice in situation.Template.ChoiceTemplates)
                {
                    prompt.AppendLine($"  * {choice.ActionTextTemplate}");
                }
            }
            prompt.AppendLine();
        }

        // Output instruction
        prompt.AppendLine("## Output Rules");
        prompt.AppendLine("1. ONE sentence only. 50-120 characters. Count carefully.");
        prompt.AppendLine("2. Pure sensory atmosphere: sight, sound, smell, or touch.");
        prompt.AppendLine("3. NO character actions or dialogue. Scene-setting ONLY.");
        prompt.AppendLine("4. Vary your openings - avoid 'dust motes', 'the air', or common clichés.");
        prompt.AppendLine("5. Entity names: Use EXACT names from context or generic terms. NEVER invent.");
        prompt.AppendLine("6. Plain text only. NO markdown, quotes, or formatting.");
        prompt.AppendLine("7. TONE PRIORITY: The Narrative Direction tone OVERRIDES weather for emotional atmosphere.");

        return prompt.ToString();
    }

    /// <summary>
    /// Build AI prompt for generating Choice action label.
    /// Contextualizes the mechanical choice template with situation narrative and entity context.
    /// Generates 5-12 word action label that player clicks.
    /// </summary>
    public string BuildChoiceLabelPrompt(
        ScenePromptContext context,
        Situation situation,
        ChoiceTemplate choiceTemplate,
        CompoundRequirement scaledRequirement,
        Consequence scaledConsequence)
    {
        StringBuilder prompt = new StringBuilder();

        prompt.AppendLine("You are a narrative writer for a Victorian-era social adventure game.");
        prompt.AppendLine("Generate a SHORT action label (5-12 words) for a player choice button.");
        prompt.AppendLine("The label should be a concrete action, not generic. Use the context provided.");
        prompt.AppendLine();

        prompt.AppendLine("## Situation Context");
        prompt.AppendLine($"- Description: {situation.Description}");
        prompt.AppendLine($"- Type: {situation.Type}");
        prompt.AppendLine();

        if (context.NPC != null)
        {
            prompt.AppendLine("## Character Present");
            prompt.AppendLine($"- Name: {context.NPC.Name}");
            prompt.AppendLine($"- Personality: {context.NPC.PersonalityType}");
            prompt.AppendLine($"- Profession: {context.NPC.Profession}");
            prompt.AppendLine();
        }

        if (context.Location != null)
        {
            prompt.AppendLine("## Location");
            prompt.AppendLine($"- Name: {context.Location.Name}");
            prompt.AppendLine($"- Purpose: {context.Location.Purpose}");
            prompt.AppendLine();
        }

        prompt.AppendLine("## Choice Mechanics");
        prompt.AppendLine($"- Template Action: {choiceTemplate.ActionTextTemplate}");
        prompt.AppendLine($"- Action Type: {choiceTemplate.ActionType}");
        prompt.AppendLine($"- Path Type: {choiceTemplate.PathType}");

        if (scaledRequirement != null && scaledRequirement.OrPaths != null && scaledRequirement.OrPaths.Count > 0)
        {
            prompt.AppendLine("- Requirements:");
            foreach (OrPath path in scaledRequirement.OrPaths)
            {
                if (path.InsightRequired.HasValue)
                    prompt.AppendLine($"  * Insight: {path.InsightRequired}");
                if (path.RapportRequired.HasValue)
                    prompt.AppendLine($"  * Rapport: {path.RapportRequired}");
                if (path.AuthorityRequired.HasValue)
                    prompt.AppendLine($"  * Authority: {path.AuthorityRequired}");
                if (path.DiplomacyRequired.HasValue)
                    prompt.AppendLine($"  * Diplomacy: {path.DiplomacyRequired}");
                if (path.CunningRequired.HasValue)
                    prompt.AppendLine($"  * Cunning: {path.CunningRequired}");
            }
        }

        if (scaledConsequence != null)
        {
            prompt.AppendLine("- Consequences:");
            if (scaledConsequence.Coins != 0)
                prompt.AppendLine($"  * Coins: {(scaledConsequence.Coins > 0 ? "+" : "")}{scaledConsequence.Coins}");
            if (scaledConsequence.Health != 0)
                prompt.AppendLine($"  * Health: {(scaledConsequence.Health > 0 ? "+" : "")}{scaledConsequence.Health}");
            if (scaledConsequence.Resolve != 0)
                prompt.AppendLine($"  * Resolve: {(scaledConsequence.Resolve > 0 ? "+" : "")}{scaledConsequence.Resolve}");
        }

        prompt.AppendLine();
        prompt.AppendLine("## Output Requirements");
        prompt.AppendLine("Write a 5-12 word action label that:");
        prompt.AppendLine("1. Is a concrete action (\"Ask Elena about lodging rates\" not \"Approach with diplomacy\")");
        prompt.AppendLine("2. Uses the NPC's name if present");
        prompt.AppendLine("3. Reflects the mechanical intent (negotiate, persuade, threaten, etc.)");
        prompt.AppendLine("4. Matches the situation's mood and context");
        prompt.AppendLine();
        prompt.AppendLine("Output ONLY the action label text, no quotes or formatting.");

        return prompt.ToString();
    }

    /// <summary>
    /// Format TimeBlock for natural language display in prompt
    /// </summary>
    private string FormatTimeBlock(TimeBlocks timeBlock)
    {
        return timeBlock switch
        {
            TimeBlocks.Morning => "Morning (early hours, fresh start)",
            TimeBlocks.Midday => "Midday (peak activity, sun overhead)",
            TimeBlocks.Afternoon => "Afternoon (winding down, shadows lengthening)",
            TimeBlocks.Evening => "Evening (darkness falls, day ends)",
            _ => timeBlock.ToString()
        };
    }

    /// <summary>
    /// Format WeatherCondition for natural language display in prompt
    /// </summary>
    private string FormatWeather(WeatherCondition weather)
    {
        return weather switch
        {
            WeatherCondition.Rain => "Rain (wet, puddles forming)",
            WeatherCondition.Storm => "Storm (thunder, lightning, heavy rain)",
            WeatherCondition.Fog => "Fog (visibility reduced, mysterious)",
            WeatherCondition.Snow => "Snow (cold, white blanket)",
            WeatherCondition.Clear => "Clear (fair weather)",
            _ => weather.ToString()
        };
    }
}
