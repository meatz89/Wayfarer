public partial class AIPromptBuilder
{
    public class TagFormatter
    {
        // Simplified tag modifications format
        public string FormatTagModifications<TKey>(Dictionary<TKey, int> tagChanges) where TKey : notnull
        {
            if (tagChanges == null || tagChanges.Count == 0)
                return "None";

            // Only include significant changes (value > 1)
            List<KeyValuePair<TKey, int>> significantChanges = tagChanges.Where(c =>
            {
                return Math.Abs(c.Value) > 1;
            }).ToList();
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

        public string FormatNarrativeTagsExtended(List<NarrativeTag> narrativeTags)
        {
            if (narrativeTags == null || narrativeTags.Count == 0)
                return "None";

            List<string> tagStrings = new List<string>();
            foreach (NarrativeTag tag in narrativeTags)
            {
                tagStrings.Add($"{tag.NarrativeName} ({tag.GetEffectDescription()}");
            }
            return string.Join(", ", tagStrings);
        }
    }
}
