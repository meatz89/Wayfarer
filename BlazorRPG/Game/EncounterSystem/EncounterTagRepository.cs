/// <summary>
/// Repository of all encounter tags in the system, including negative location reaction tags
/// </summary>
public class EncounterTagRepository
{
    private readonly Dictionary<string, EncounterTag> _tags = new Dictionary<string, EncounterTag>();

    public EncounterTagRepository()
    {
        InitializePlayerTags();
        InitializeLocationReactionTags();
    }

    public EncounterTag GetTag(string id)
    {
        return _tags.ContainsKey(id) ? _tags[id] : null;
    }

    // New method for type-safe tag retrieval
    public EncounterTag GetDominanceTag(string dominanceTagId)
    {
        // Validate that the ID is indeed a Dominance tag
        if (!TagRegistry.Dominance.AllTags.Contains(dominanceTagId))
        {
            throw new ArgumentException($"Invalid Dominance tag ID: {dominanceTagId}");
        }
        return GetTag(dominanceTagId);
    }

    // More type-safe retrieval methods for each tag category
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
        // This method validates that we're using a proper location reaction tag
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
    /// Initialize positive player tags (threshold-based)
    /// </summary>
    private void InitializePlayerTags()
    {
        // DOMINANCE TAGS (Force approach)

        // Level 3 Dominance Tags
        AddTag(
            TagRegistry.Dominance.IntimidationTactics,
            "Dominance Boost: Physical Focus (+1 momentum)",
            "Physical-related choices give +1 additional momentum",
            SignatureElementTypes.Dominance,
            3,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Physical,
                MomentumModifier = 1
            }
        );

        AddTag(
            TagRegistry.Dominance.ForcefulAuthority,
            "Dominance Shield: Relationship Focus (no pressure)",
            "Relationship-related choices generate no pressure",
            SignatureElementTypes.Dominance,
            3,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Relationship,
                ZeroPressure = true
            }
        );

        // Additional tag definitions continue with the same pattern...
        // For brevity, I'm including just a sample of the tag definitions
        // The full implementation would include all tags defined in the TagRegistry
    }

    /// <summary>
    /// Initialize negative location reaction tags
    /// </summary>
    private void InitializeLocationReactionTags()
    {
        // Merchant Guild negative tags

        // Social Faux Pas - Triggered by Force approach in the refined Guild
        EncounterTag socialFauxPasTag = new EncounterTag(
            TagRegistry.LocationReaction.SocialFauxPas,
            "Momentum Block: Relationship Focus (0 momentum)",
            "Your aggressive manner has offended the merchants. Relationship-focused choices generate no momentum.",
            SignatureElementTypes.Rapport,
            0,
            new TagEffect
            {
                IsNegative = true,
                AffectedFocus = FocusTypes.Relationship,
                BlockMomentum = true
            }
        );

        socialFauxPasTag.ActivationTriggers.Add(new TagTrigger(
            "force_guild_trigger",
            "Using Force in the Merchant Guild is a social blunder",
            ApproachTypes.Force
        ));

        socialFauxPasTag.RemovalTriggers.Add(new TagTrigger(
            "charm_removal",
            "Charm can help recover from social mistakes",
            ApproachTypes.Charm
        ));

        socialFauxPasTag.IsLocationReaction = true;
        _tags[socialFauxPasTag.Id] = socialFauxPasTag;

        // Additional location reaction tag definitions continue with the same pattern...
        // For brevity, I'm including just a sample of the tag definitions
        // The full implementation would include all location reaction tags defined in the TagRegistry
    }

    private void AddTag(string id, string name, string description, SignatureElementTypes sourceElement,
                      int thresholdValue, TagEffect effect)
    {
        _tags[id] = new EncounterTag(id, name, description, sourceElement, thresholdValue, effect);
    }
}