// Formats tags for presentation in prompts
using BlazorRPG.Game.EncounterManager;
public class TagFormatter
{
    public (string, string) GetSignificantApproachTags(EncounterStatus state)
    {
        List<KeyValuePair<ApproachTags, int>> orderedTags = state.ApproachTags
            .OrderByDescending(t => t.Value)
            .ToList();
        string primary = orderedTags.Count > 0 ? orderedTags[0].Key.ToString() : "None";
        string secondary = orderedTags.Count > 1 ? orderedTags[1].Key.ToString() : "None";
        return (primary, secondary);
    }

    public (string, string) GetSignificantFocusTags(EncounterStatus state)
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
        foreach (KeyValuePair<EncounterStateTags, int> change in projection.EncounterStateTagChanges.Where(c => c.Value != 0))
        {
            changes.Add($"{change.Key} {(change.Value > 0 ? "+" : "")}{change.Value}");
        }
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
        foreach (var tag in tags)
        {
            // Include all tags, even those with 0 value
            tagStrings.Add($"{tag.Key}: {tag.Value}");
        }
        return string.Join(", ", tagStrings);
    }

    public string FormatTagModifications<TKey>(Dictionary<TKey, int> tagChanges) where TKey : notnull
    {
        if (tagChanges.Count == 0)
            return "None";

        List<string> modificationStrings = new List<string>();
        foreach (var change in tagChanges)
        {
            string direction = change.Value > 0 ? "increased" : "decreased";
            modificationStrings.Add($"{change.Key} {direction} by {Math.Abs(change.Value)}");
        }
        return string.Join(", ", modificationStrings);
    }

    public string FormatNarrativeTags(EncounterStatus state)
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
    public string FormatStrategicTags(EncounterStatus state)
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
        foreach (var tag in narrativeTags)
        {
            if (tag.BlockedApproach.HasValue)
            {
                tagStrings.Add($"{tag.Name} (Blocks {tag.BlockedApproach.Value} approaches)");
            }
            else
            {
                tagStrings.Add($"{tag.Name} (Narrative effect)");
            }
        }
        return string.Join(", ", tagStrings);
    }

    // Format strategic tags with effect descriptions - for when you have direct access to StrategicTag objects
    public string FormatStrategicTagsExtended(List<StrategicTag> strategicTags)
    {
        if (strategicTags == null || strategicTags.Count == 0)
            return "None";

        List<string> tagStrings = new List<string>();
        foreach (var tag in strategicTags)
        {
            string effectDescription = "Strategic effect";
            switch (tag.EffectType)
            {
                case StrategicEffectTypes.AddMomentumToApproach:
                    effectDescription = $"+{tag.EffectValue} momentum to {tag.AffectedApproach} approaches";
                    break;
                case StrategicEffectTypes.AddMomentumToFocus:
                    effectDescription = $"+{tag.EffectValue} momentum to {tag.AffectedFocus} focuses";
                    break;
                case StrategicEffectTypes.ReducePressureFromApproach:
                    effectDescription = $"-{tag.EffectValue} pressure from {tag.AffectedApproach} approaches";
                    break;
                case StrategicEffectTypes.ReducePressureFromFocus:
                    effectDescription = $"-{tag.EffectValue} pressure from {tag.AffectedFocus} focuses";
                    break;
                case StrategicEffectTypes.ReducePressurePerTurn:
                    effectDescription = $"-{tag.EffectValue} pressure each turn";
                    break;
                case StrategicEffectTypes.AddPressurePerTurn:
                    effectDescription = $"+{tag.EffectValue} pressure each turn";
                    break;
                case StrategicEffectTypes.ReduceHealthByPressure:
                    effectDescription = $"Reduces health based on pressure";
                    break;
                case StrategicEffectTypes.ReduceHealthByApproachValue:
                    effectDescription = $"Reduces health based on {tag.ScalingApproachTag} value";
                    break;
            }

            tagStrings.Add($"{tag.Name} ({effectDescription})");
        }
        return string.Join(", ", tagStrings);
    }
}