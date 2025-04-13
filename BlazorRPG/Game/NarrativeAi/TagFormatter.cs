// Formats tags for presentation in prompts

public class TagFormatter
{

    // Format significant tags - keep this existing method but enhance it
    public string GetSignificantTagsFormatted(EncounterStatusModel state)
    {
        List<string> significantTags = new List<string>();

        // Only include approach tags with values of 2 or higher
        foreach (KeyValuePair<ApproachTags, int> tag in state.ApproachTags.Where(t => t.Value >= 2).OrderByDescending(t => t.Value))
        {
            significantTags.Add($"{tag.Key} {tag.Value}");
        }
        // Only include approach tags with values of 2 or higher
        foreach (KeyValuePair<ApproachTags, int> tag in state.ApproachTags.Where(t => t.Value >= 2).OrderByDescending(t => t.Value))
        {
            significantTags.Add($"{tag.Key} {tag.Value}");
        }
        // Only include focus tags with values of 2 or higher
        foreach (KeyValuePair<FocusTags, int> tag in state.FocusTags.Where(t => t.Value >= 2).OrderByDescending(t => t.Value))
        {
            significantTags.Add($"{tag.Key} {tag.Value}");
        }

        // Include only active tags that have gameplay effects
        foreach (string tag in state.ActiveTagNames)
        {
            // Add a note for important tag effects
            if (tag.Contains("Open Marketplace"))
                significantTags.Add($"{tag} (blocks Stealth)");
            else if (tag.Contains("Drawn Weapons"))
                significantTags.Add($"{tag} (blocks Wit)");
            else if (tag.Contains("Hostile Territory"))
                significantTags.Add($"{tag} (blocks Charm)");
            else if (tag.Contains("Fight Started"))
                significantTags.Add($"{tag} (only Force allowed)");
            else if (tag.Contains("Tension"))
                significantTags.Add($"{tag} (adds pressure)");
            else if (tag.Contains("Coordinated"))
                significantTags.Add($"{tag} (boosts Force)");
            else if (tag.Contains("Distracted"))
                significantTags.Add($"{tag} (boosts Stealth)");
            else
                significantTags.Add(tag);
        }

        return significantTags.Count > 0 ? string.Join(", ", significantTags) : "None";
    }

    // Simplified tag modifications format
    public string FormatTagModifications<TKey>(Dictionary<TKey, int> tagChanges) where TKey : notnull
    {
        if (tagChanges == null || tagChanges.Count == 0)
            return "None";

        // Only include significant changes (value > 1)
        List<KeyValuePair<TKey, int>> significantChanges = tagChanges.Where(c => Math.Abs(c.Value) > 1).ToList();
        if (significantChanges.Count == 0)
            return "Minor changes only";

        List<string> modificationStrings = new List<string>();
        foreach (KeyValuePair<TKey, int> change in significantChanges)
        {
            string direction = change.Value > 0 ? "increased" : "decreased";
            modificationStrings.Add($"{change.Key} {direction} by {Math.Abs(change.Value)}");
        }

        return string.Join(", ", modificationStrings);
    }

    // Simplified active narrative tags format
    public string FormatActiveNarrativeTags(EncounterStatusModel state)
    {
        // Only include narrative tags that block approaches
        List<string> activeTagsWithEffects = new List<string>();

        foreach (string tag in state.ActiveTagNames)
        {
            if (tag.Contains("Open Marketplace"))
                activeTagsWithEffects.Add($"{tag} (blocks Stealth approaches)");
            else if (tag.Contains("Drawn Weapons"))
                activeTagsWithEffects.Add($"{tag} (blocks Wit approaches)");
            else if (tag.Contains("Hostile Territory"))
                activeTagsWithEffects.Add($"{tag} (blocks Charm approaches)");
            else if (tag.Contains("Fight Started"))
                activeTagsWithEffects.Add($"{tag} (only Force approaches allowed)");
        }

        return activeTagsWithEffects.Count > 0 ?
            string.Join(", ", activeTagsWithEffects) :
            "No approach restrictions";
    }

    public (string, string) GetSignificantApproachTags(EncounterStatusModel state)
    {
        List<KeyValuePair<ApproachTags, int>> orderedTags = state.ApproachTags
            .OrderByDescending(t => t.Value)
            .ToList();
        string primary = orderedTags.Count > 0 ? orderedTags[0].Key.ToString() : "None";
        string secondary = orderedTags.Count > 1 ? orderedTags[1].Key.ToString() : "None";
        return (primary, secondary);
    }

    public (string, string) GetSignificantFocusTags(EncounterStatusModel state)
    {
        List<KeyValuePair<FocusTags, int>> orderedTags = state.FocusTags
            .OrderByDescending(t => t.Value)
            .ToList();
        string primary = orderedTags.Count > 0 ? orderedTags[0].Key.ToString() : "None";
        string secondary = orderedTags.Count > 1 ? orderedTags[1].Key.ToString() : "None";
        return (primary, secondary);
    }

    public string FormatKeyTagChanges(ChoiceProjection projection)
    {
        List<string> changes = new List<string>();
        // Format approach tag changes
        foreach (KeyValuePair<ApproachTags, int> change in projection.ApproachTagChanges.Where(c => c.Value != 0))
        {
            changes.Add($"{change.Key} {(change.Value > 0 ? "+" : "")}{change.Value}");
        }
        // Format focus tag changes
        foreach (KeyValuePair<FocusTags, int> change in projection.FocusTagChanges.Where(c => c.Value != 0))
        {
            changes.Add($"{change.Key} {(change.Value > 0 ? "+" : "")}{change.Value}");
        }
        return changes.Count > 0 ? string.Join(", ", changes) : "No significant changes";
    }

    public string FormatTagValues<TKey>(Dictionary<TKey, int> tags) where TKey : notnull
    {
        List<string> tagStrings = new List<string>();
        foreach (KeyValuePair<TKey, int> tag in tags)
        {
            // Include all tags, even those with 0 value
            tagStrings.Add($"{tag.Key}: {tag.Value}");
        }
        return string.Join(", ", tagStrings);
    }

    public string FormatNarrativeTags(EncounterStatusModel state)
    {
        // Since EncounterStatus doesn't expose tag objects directly, work with names
        List<string> activeNarrativeTagNames = state.ActiveTagNames
            .Where(name => name.Contains("Market") || name.Contains("Territory") ||
                           name.Contains("Fight") || name.Contains("Weapons"))
            .ToList();

        if (activeNarrativeTagNames.Count == 0)
            return "None";

        List<string> tagDescriptions = new List<string>();
        foreach (string tagName in activeNarrativeTagNames)
        {
            // Add basic descriptions for known narrative tags
            if (tagName.Contains("Open Marketplace"))
                tagDescriptions.Add($"{tagName} (Blocks Stealth approaches)");
            else if (tagName.Contains("Drawn Weapons"))
                tagDescriptions.Add($"{tagName} (Blocks Wit approaches)");
            else if (tagName.Contains("Hostile Territory"))
                tagDescriptions.Add($"{tagName} (Blocks Charm approaches)");
            else if (tagName.Contains("Fight Started"))
                tagDescriptions.Add($"{tagName} (Blocks non-Force approaches)");
            else
                tagDescriptions.Add($"{tagName} (Narrative effect)");
        }

        return string.Join(", ", tagDescriptions);
    }

    // Format strategic tags with effect descriptions
    public string FormatStrategicTags(EncounterStatusModel state)
    {
        // Since EncounterStatus doesn't expose tag objects directly, work with names
        List<string> activeStrategicTagNames = state.ActiveTagNames
            .Where(name => !name.Contains("Market") && !name.Contains("Territory") &&
                          !name.Contains("Fight") && !name.Contains("Weapons"))
            .ToList();

        if (activeStrategicTagNames.Count == 0)
            return "None";

        List<string> tagDescriptions = new List<string>();
        foreach (string tagName in activeStrategicTagNames)
        {
            // Add basic descriptions for known strategic tags
            if (tagName.Contains("Tension"))
                tagDescriptions.Add($"{tagName} (Adds pressure each turn)");
            else if (tagName.Contains("Coordinated"))
                tagDescriptions.Add($"{tagName} (Adds momentum to Force approaches)");
            else if (tagName.Contains("Distracted"))
                tagDescriptions.Add($"{tagName} (Adds momentum to Stealth approaches)");
            else if (tagName.Contains("Exhaustion"))
                tagDescriptions.Add($"{tagName} (Reduces health based on pressure)");
            else
                tagDescriptions.Add($"{tagName} (Strategic effect)");
        }

        return string.Join(", ", tagDescriptions);
    }

    // Add these methods to your TagFormatter class

    // Format narrative tags with effect descriptions - for when you have direct access to NarrativeTag objects
    public string FormatNarrativeTagsExtended(List<NarrativeTag> narrativeTags)
    {
        if (narrativeTags == null || narrativeTags.Count == 0)
            return "None";

        List<string> tagStrings = new List<string>();
        foreach (NarrativeTag tag in narrativeTags)
        {
            if (tag.BlockedFocus != null)
            {
                tagStrings.Add($"{tag.NarrativeName} (Blocks {tag.BlockedFocus} focus)");
            }
            else
            {
                tagStrings.Add($"{tag.NarrativeName} (Narrative effect)");
            }
        }
        return string.Join(", ", tagStrings);
    }

}