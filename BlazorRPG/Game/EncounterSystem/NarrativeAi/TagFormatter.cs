
// Formats tags for presentation in prompts
using BlazorRPG.Game.EncounterManager;

public class TagFormatter
{
    public string GetSignificantTagsFormatted(EncounterStatus state)
    {
        List<string> significantTags = new List<string>();

        // Get high approach tags
        foreach (KeyValuePair<EncounterStateTags, int> tag in state.ApproachTags.Where(t => t.Value >= 2).OrderByDescending(t => t.Value))
        {
            significantTags.Add($"{tag.Key} {tag.Value}{(tag.Value >= 4 ? " (MAJOR)" : "")}");
        }

        // Get high focus tags
        foreach (KeyValuePair<FocusTags, int> tag in state.FocusTags.Where(t => t.Value >= 2).OrderByDescending(t => t.Value))
        {
            significantTags.Add($"{tag.Key} {tag.Value}{(tag.Value >= 4 ? " (MAJOR)" : "")}");
        }

        // Get active tags
        foreach (string tag in state.ActiveTagNames)
        {
            significantTags.Add($"{tag} (ACTIVE)");
        }

        return string.Join(", ", significantTags);
    }

    public (string, string) GetSignificantApproachTags(EncounterStatus state)
    {
        List<KeyValuePair<EncounterStateTags, int>> orderedTags = state.ApproachTags
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
}
