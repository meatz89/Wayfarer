/// <summary>
/// Extended builder for location strategic properties with improved type safety
/// </summary>
public class LocationStrategicBuilder
{
    private string _id;
    private string _name;
    private List<SignatureElementTypes> _favoredElements;
    private List<SignatureElementTypes> _disfavoredElements;
    private List<string> _availableTagIds;
    private List<string> _locationReactionTagIds;

    public LocationStrategicBuilder()
    {
        _id = string.Empty;
        _name = string.Empty;
        _favoredElements = new List<SignatureElementTypes>();
        _disfavoredElements = new List<SignatureElementTypes>();
        _availableTagIds = new List<string>();
        _locationReactionTagIds = new List<string>();
    }

    public LocationStrategicBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public LocationStrategicBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public LocationStrategicBuilder WithFavoredElement(SignatureElementTypes element)
    {
        _favoredElements.Add(element);
        return this;
    }

    public LocationStrategicBuilder WithDisfavoredElement(SignatureElementTypes element)
    {
        _disfavoredElements.Add(element);
        return this;
    }

    public LocationStrategicBuilder WithAvailableTag(string tagId)
    {
        _availableTagIds.Add(tagId);
        return this;
    }

    public LocationStrategicBuilder WithLocationReactionTag(string tagId)
    {
        _locationReactionTagIds.Add(tagId);
        return this;
    }

    // New strongly-typed methods for adding tags

    // Dominance Tags
    public LocationStrategicBuilder WithDominanceTag(string dominanceTagId)
    {
        if (!TagRegistry.Dominance.AllTags.Contains(dominanceTagId))
        {
            throw new ArgumentException($"Invalid Dominance tag ID: {dominanceTagId}");
        }
        return WithAvailableTag(dominanceTagId);
    }

    // Rapport Tags
    public LocationStrategicBuilder WithRapportTag(string rapportTagId)
    {
        if (!TagRegistry.Rapport.AllTags.Contains(rapportTagId))
        {
            throw new ArgumentException($"Invalid Rapport tag ID: {rapportTagId}");
        }
        return WithAvailableTag(rapportTagId);
    }

    // Analysis Tags
    public LocationStrategicBuilder WithAnalysisTag(string analysisTagId)
    {
        if (!TagRegistry.Analysis.AllTags.Contains(analysisTagId))
        {
            throw new ArgumentException($"Invalid Analysis tag ID: {analysisTagId}");
        }
        return WithAvailableTag(analysisTagId);
    }

    // Precision Tags
    public LocationStrategicBuilder WithPrecisionTag(string precisionTagId)
    {
        if (!TagRegistry.Precision.AllTags.Contains(precisionTagId))
        {
            throw new ArgumentException($"Invalid Precision tag ID: {precisionTagId}");
        }
        return WithAvailableTag(precisionTagId);
    }

    // Concealment Tags
    public LocationStrategicBuilder WithConcealmentTag(string concealmentTagId)
    {
        if (!TagRegistry.Concealment.AllTags.Contains(concealmentTagId))
        {
            throw new ArgumentException($"Invalid Concealment tag ID: {concealmentTagId}");
        }
        return WithAvailableTag(concealmentTagId);
    }

    // Bulk tag methods
    public LocationStrategicBuilder WithDominanceTags(params string[] tagIds)
    {
        foreach (var tagId in tagIds)
        {
            WithDominanceTag(tagId);
        }
        return this;
    }

    public LocationStrategicBuilder WithRapportTags(params string[] tagIds)
    {
        foreach (var tagId in tagIds)
        {
            WithRapportTag(tagId);
        }
        return this;
    }

    public LocationStrategicBuilder WithAnalysisTags(params string[] tagIds)
    {
        foreach (var tagId in tagIds)
        {
            WithAnalysisTag(tagId);
        }
        return this;
    }

    public LocationStrategicBuilder WithPrecisionTags(params string[] tagIds)
    {
        foreach (var tagId in tagIds)
        {
            WithPrecisionTag(tagId);
        }
        return this;
    }

    public LocationStrategicBuilder WithConcealmentTags(params string[] tagIds)
    {
        foreach (var tagId in tagIds)
        {
            WithConcealmentTag(tagId);
        }
        return this;
    }

    public LocationStrategicProperties Build()
    {
        return new LocationStrategicProperties(
            _id,
            _name,
            _favoredElements,
            _disfavoredElements,
            _availableTagIds,
            _locationReactionTagIds
        );
    }
}