/// <summary>
/// Repository of all encounter tags in the system, including negative location reaction tags
/// </summary>
public class EncounterTagRepository
{
    private readonly Dictionary<string, EncounterTag> _tags = new Dictionary<string, EncounterTag>();

    public EncounterTagRepository()
    {
        // Use the content factory to populate the repository
        TagContentFactory.CreateAllTags(_tags);

        // Validate that all defined tags are initialized
        ValidateRepository();
    }

    public EncounterTag GetTag(string id)
    {
        return _tags.ContainsKey(id) ? _tags[id] : null;
    }

    // Type-safe tag retrieval methods
    public EncounterTag GetDominanceTag(string dominanceTagId)
    {
        if (!TagRegistry.Dominance.AllTags.Contains(dominanceTagId))
        {
            throw new ArgumentException($"Invalid Dominance tag ID: {dominanceTagId}");
        }
        return GetTag(dominanceTagId);
    }

    public EncounterTag GetRapportTag(string rapportTagId)
    {
        if (!TagRegistry.Rapport.AllTags.Contains(rapportTagId))
        {
            throw new ArgumentException($"Invalid Rapport tag ID: {rapportTagId}");
        }
        return GetTag(rapportTagId);
    }

    public EncounterTag GetAnalysisTag(string analysisTagId)
    {
        if (!TagRegistry.Analysis.AllTags.Contains(analysisTagId))
        {
            throw new ArgumentException($"Invalid Analysis tag ID: {analysisTagId}");
        }
        return GetTag(analysisTagId);
    }

    public EncounterTag GetPrecisionTag(string precisionTagId)
    {
        if (!TagRegistry.Precision.AllTags.Contains(precisionTagId))
        {
            throw new ArgumentException($"Invalid Precision tag ID: {precisionTagId}");
        }
        return GetTag(precisionTagId);
    }

    public EncounterTag GetConcealmentTag(string concealmentTagId)
    {
        if (!TagRegistry.Concealment.AllTags.Contains(concealmentTagId))
        {
            throw new ArgumentException($"Invalid Concealment tag ID: {concealmentTagId}");
        }
        return GetTag(concealmentTagId);
    }

    public EncounterTag GetLocationReactionTag(string locationReactionTagId)
    {
        bool isAnyLocationTag =
            TagRegistry.LocationReaction.MerchantGuild.Contains(locationReactionTagId) ||
            TagRegistry.LocationReaction.BanditCamp.Contains(locationReactionTagId);

        if (!isAnyLocationTag)
        {
            throw new ArgumentException($"Invalid Location Reaction tag ID: {locationReactionTagId}");
        }
        return GetTag(locationReactionTagId);
    }

    public IEnumerable<EncounterTag> GetAllTags()
    {
        return _tags.Values;
    }

    public IEnumerable<EncounterTag> GetTagsByElement(SignatureElementTypes elementType)
    {
        return _tags.Values.Where(t => t.SourceElement == elementType);
    }

    public IEnumerable<EncounterTag> GetLocationReactionTags()
    {
        return _tags.Values.Where(t => t.IsLocationReaction);
    }

    /// <summary>
    /// Adds a new tag to the repository (for runtime tag creation)
    /// </summary>
    public void AddTag(EncounterTag tag)
    {
        if (tag == null)
            throw new ArgumentNullException(nameof(tag));

        if (_tags.ContainsKey(tag.Id))
            throw new ArgumentException($"A tag with ID '{tag.Id}' already exists in the repository");

        _tags[tag.Id] = tag;
    }

    /// <summary>
    /// Validates that all tags defined in TagRegistry are initialized in the repository
    /// </summary>
    private void ValidateRepository()
    {
        List<string> missingTags = new List<string>();

        // Check Dominance tags
        foreach (string tagId in TagRegistry.Dominance.AllTags)
        {
            if (!_tags.ContainsKey(tagId))
                missingTags.Add(tagId);
        }

        // Check Rapport tags
        foreach (string tagId in TagRegistry.Rapport.AllTags)
        {
            if (!_tags.ContainsKey(tagId))
                missingTags.Add(tagId);
        }

        // Check Analysis tags
        foreach (string tagId in TagRegistry.Analysis.AllTags)
        {
            if (!_tags.ContainsKey(tagId))
                missingTags.Add(tagId);
        }

        // Check Precision tags
        foreach (string tagId in TagRegistry.Precision.AllTags)
        {
            if (!_tags.ContainsKey(tagId))
                missingTags.Add(tagId);
        }

        // Check Concealment tags
        foreach (string tagId in TagRegistry.Concealment.AllTags)
        {
            if (!_tags.ContainsKey(tagId))
                missingTags.Add(tagId);
        }

        // Check Location Reaction tags
        foreach (string tagId in TagRegistry.LocationReaction.MerchantGuild)
        {
            if (!_tags.ContainsKey(tagId))
                missingTags.Add(tagId);
        }

        foreach (string tagId in TagRegistry.LocationReaction.BanditCamp)
        {
            if (!_tags.ContainsKey(tagId))
                missingTags.Add(tagId);
        }

        // If any tags are missing, throw an exception
        if (missingTags.Count > 0)
        {
            throw new InvalidOperationException(
                $"The following tags are defined in TagRegistry but not initialized in the repository: {string.Join(", ", missingTags)}"
            );
        }
    }
}