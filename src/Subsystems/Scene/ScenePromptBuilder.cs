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
    /// Build AI prompt for generating Situation narrative description WITH FRICTION.
    /// Creates 3-5 sentences (250-350 chars MAX) that present a problem or tension.
    /// The situation should set up WHY the player needs to make a choice.
    /// </summary>
    public string BuildSituationPrompt(ScenePromptContext context, NarrativeHints hints, Situation situation)
    {
        StringBuilder prompt = new StringBuilder();

        // System instruction - FRICTION-FOCUSED
        prompt.AppendLine("You are a narrative writer for a Victorian-era social adventure game.");
        prompt.AppendLine("Generate 3-5 sentences (250-350 characters MAX) that present a SITUATION WITH FRICTION.");
        prompt.AppendLine("The player must face a problem, obstacle, or tension that requires a decision.");
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

        // NPC context - use NARRATIVE relationship descriptors, not mechanical terms
        if (context.NPC != null)
        {
            prompt.AppendLine("## Character Present");
            prompt.AppendLine($"- Name: {context.NPC.Name}");
            prompt.AppendLine($"- Personality: {context.NPC.PersonalityType}");
            prompt.AppendLine($"- Profession: {context.NPC.Profession}");
            prompt.AppendLine($"- Current State: {context.NPC.CurrentState}");
            if (context.NPCBondLevel != 0)
                prompt.AppendLine($"- Relationship: {FormatRelationshipNarratively(context.NPCBondLevel)}");
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

        // Output instruction - FRICTION-FOCUSED
        prompt.AppendLine("## Output Rules");
        prompt.AppendLine("1. Write 3-5 sentences. STRICT LIMIT: 250-350 characters. Count EVERY character.");
        prompt.AppendLine("2. FIRST: Set the scene with one vivid sensory detail.");
        prompt.AppendLine("3. THEN: Present the FRICTION - what problem/obstacle/tension does the player face?");
        prompt.AppendLine("4. The friction should make the player WANT to choose - there's something at stake.");
        prompt.AppendLine("5. Entity names: Use EXACT names from context or generic terms. NEVER invent.");
        prompt.AppendLine("6. Plain text ONLY. NO markdown, NO smart quotes (\"), NO curly quotes, NO asterisks, NO formatting.");
        prompt.AppendLine("7. TONE PRIORITY: The Narrative Direction tone OVERRIDES weather for emotional atmosphere.");
        prompt.AppendLine("8. End with the tension unresolved - the choice comes next.");
        prompt.AppendLine("9. NEVER reference game mechanics (bond levels, stats, resources). Use NARRATIVE language only.");

        return prompt.ToString();
    }

    /// <summary>
    /// Build AI prompt for generating ALL choice labels in a single call.
    /// Ensures choices are narratively differentiated based on mechanical context.
    /// Returns JSON array format for parsing.
    /// </summary>
    public string BuildBatchChoiceLabelsPrompt(
        ScenePromptContext context,
        Situation situation,
        List<ChoiceData> choicesData)
    {
        StringBuilder prompt = new StringBuilder();

        prompt.AppendLine("You are a narrative writer for a Victorian-era social adventure game.");
        prompt.AppendLine("Generate action labels for ALL choices below IN ONE RESPONSE.");
        prompt.AppendLine("Each choice must be NARRATIVELY DISTINCT - different approaches to the same situation.");
        prompt.AppendLine();

        // Situation context (with friction)
        prompt.AppendLine("## The Situation");
        prompt.AppendLine(situation.Description);
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
            prompt.AppendLine();
        }

        // All choices with their mechanical context
        prompt.AppendLine("## Choices to Label");
        prompt.AppendLine("Each choice has different mechanical requirements and consequences.");
        prompt.AppendLine("Use these differences to create DISTINCT narrative approaches:");
        prompt.AppendLine();

        for (int i = 0; i < choicesData.Count; i++)
        {
            ChoiceData data = choicesData[i];
            prompt.AppendLine($"### Choice {i + 1}: {data.Template.ActionTextTemplate}");
            prompt.AppendLine($"- Path Type: {data.Template.PathType}");

            // Requirements show WHAT STAT is needed - this shapes the approach
            if (data.Requirement != null && data.Requirement.OrPaths != null && data.Requirement.OrPaths.Count > 0)
            {
                prompt.AppendLine("- Requirements (stat needed = approach type):");
                foreach (OrPath path in data.Requirement.OrPaths)
                {
                    if (path.InsightRequired.HasValue)
                        prompt.AppendLine($"  * Insight {path.InsightRequired} = observant, analytical approach");
                    if (path.RapportRequired.HasValue)
                        prompt.AppendLine($"  * Rapport {path.RapportRequired} = friendly, empathetic approach");
                    if (path.AuthorityRequired.HasValue)
                        prompt.AppendLine($"  * Authority {path.AuthorityRequired} = commanding, status-based approach");
                    if (path.DiplomacyRequired.HasValue)
                        prompt.AppendLine($"  * Diplomacy {path.DiplomacyRequired} = tactful, negotiating approach");
                    if (path.CunningRequired.HasValue)
                        prompt.AppendLine($"  * Cunning {path.CunningRequired} = clever, manipulative approach");
                }
            }

            // Consequences show STAKES - cost vs reward
            if (data.Consequence != null)
            {
                List<string> stakes = new List<string>();
                if (data.Consequence.Coins != 0)
                    stakes.Add($"Coins {(data.Consequence.Coins > 0 ? "+" : "")}{data.Consequence.Coins}");
                if (data.Consequence.Health != 0)
                    stakes.Add($"Health {(data.Consequence.Health > 0 ? "+" : "")}{data.Consequence.Health}");
                if (data.Consequence.Resolve != 0)
                    stakes.Add($"Resolve {(data.Consequence.Resolve > 0 ? "+" : "")}{data.Consequence.Resolve}");
                if (stakes.Count > 0)
                    prompt.AppendLine($"- Stakes: {string.Join(", ", stakes)}");
            }
            prompt.AppendLine();
        }

        // Output format
        prompt.AppendLine("## Output Format");
        prompt.AppendLine("Return a JSON object with a 'choices' array containing EXACTLY the labels in order:");
        prompt.AppendLine("```json");
        prompt.AppendLine("{");
        prompt.AppendLine("  \"choices\": [");
        prompt.AppendLine("    \"Label for choice 1 (5-12 words)\",");
        prompt.AppendLine("    \"Label for choice 2 (5-12 words)\",");
        prompt.AppendLine("    \"Label for choice 3 (5-12 words)\",");
        prompt.AppendLine("    \"Label for choice 4 (5-12 words)\"");
        prompt.AppendLine("  ]");
        prompt.AppendLine("}");
        prompt.AppendLine("```");
        prompt.AppendLine();
        prompt.AppendLine("## Rules");
        prompt.AppendLine("1. Each label is 5-12 words, a concrete action");
        prompt.AppendLine("2. Use the NPC's name if present");
        prompt.AppendLine("3. Make each choice feel like a DIFFERENT approach to the friction");
        prompt.AppendLine("4. The stat requirement hints at HOW the player approaches (cunning=trick, authority=demand)");
        prompt.AppendLine("5. NO generic labels like 'Approach diplomatically' - be SPECIFIC to the situation");
        prompt.AppendLine("6. Output ONLY the JSON, no other text");

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

    /// <summary>
    /// Convert numerical bond level to NARRATIVE relationship description.
    /// Prevents AI from echoing game mechanics like "bond level 3".
    /// </summary>
    private string FormatRelationshipNarratively(int bondLevel)
    {
        return bondLevel switch
        {
            <= -3 => "openly hostile, bitter enemies",
            -2 => "deeply distrustful, resentful",
            -1 => "wary, somewhat suspicious",
            0 => "neutral acquaintances",
            1 => "friendly, on good terms",
            2 => "warm rapport, genuine trust",
            >= 3 => "close confidants, deep bond"
        };
    }
}

/// <summary>
/// Data container for batch choice label generation.
/// Holds the template and scaled mechanical values for a single choice.
/// </summary>
public class ChoiceData
{
    public ChoiceTemplate Template { get; set; }
    public CompoundRequirement Requirement { get; set; }
    public Consequence Consequence { get; set; }

    public ChoiceData(ChoiceTemplate template, CompoundRequirement requirement, Consequence consequence)
    {
        Template = template;
        Requirement = requirement;
        Consequence = consequence;
    }
}
