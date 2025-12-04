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
        prompt.AppendLine("Generate a brief, atmospheric description (2-3 sentences) for a game situation.");
        prompt.AppendLine("Focus on mood, tension, and character dynamics. Be evocative but concise.");
        prompt.AppendLine();

        // World context
        prompt.AppendLine("## World Context");
        prompt.AppendLine($"- Time: {FormatTimeBlock(context.CurrentTimeBlock)}");
        if (!string.IsNullOrEmpty(context.CurrentWeather))
            prompt.AppendLine($"- Weather: {context.CurrentWeather}");
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
        prompt.AppendLine("## Output");
        prompt.AppendLine("Write a 2-3 sentence atmospheric description that:");
        prompt.AppendLine("1. Sets the scene with sensory details");
        prompt.AppendLine("2. Establishes the tension or mood");
        prompt.AppendLine("3. Hints at what the player might do without being prescriptive");
        prompt.AppendLine();
        prompt.AppendLine("Output ONLY the narrative description, no JSON or formatting.");

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
}
