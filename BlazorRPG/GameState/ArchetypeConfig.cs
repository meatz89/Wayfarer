public class ArchetypeConfig
{
    // Primary approach for this archetype
    public ApproachTags PrimaryApproach { get; private set; }

    // Fixed-size array of affinities for all approach contexts
    public AffinityTypes[] AffinityValues;

    // Static factory method for Warrior archetype
    public static ArchetypeConfig CreateWarrior()
    {
        ArchetypeConfig config = new ArchetypeConfig();
        config.PrimaryApproach = ApproachTags.Dominance;

        // Initialize with default neutral values
        config.AffinityValues = new AffinityTypes[Enum.GetValues(typeof(ApproachContexts)).Length];
        for (int i = 0; i < config.AffinityValues.Length; i++)
        {
            config.AffinityValues[i] = AffinityTypes.Neutral;
        }

        // Set warrior-specific affinities

        // Natural affinities
        config.SetAffinity(ApproachTags.Dominance, EncounterTypes.Physical, AffinityTypes.Natural);
        config.SetAffinity(ApproachTags.Dominance, EncounterTypes.Social, AffinityTypes.Natural);

        // Unnatural affinities
        config.SetAffinity(ApproachTags.Analysis, EncounterTypes.Physical, AffinityTypes.Unnatural);
        config.SetAffinity(ApproachTags.Analysis, EncounterTypes.Social, AffinityTypes.Unnatural);
        config.SetAffinity(ApproachTags.Rapport, EncounterTypes.Physical, AffinityTypes.Unnatural);

        return config;
    }

    public static ArchetypeConfig CreateScholar()
    {
        ArchetypeConfig config = new ArchetypeConfig();
        config.PrimaryApproach = ApproachTags.Analysis;

        // Initialize with default neutral values
        config.AffinityValues = new AffinityTypes[Enum.GetValues(typeof(ApproachContexts)).Length];
        for (int i = 0; i < config.AffinityValues.Length; i++)
        {
            config.AffinityValues[i] = AffinityTypes.Neutral;
        }

        // Set scholar-specific affinities

        // Natural affinities
        config.SetAffinity(ApproachTags.Analysis, EncounterTypes.Intellectual, AffinityTypes.Natural);
        config.SetAffinity(ApproachTags.Analysis, EncounterTypes.Physical, AffinityTypes.Natural);
        config.SetAffinity(ApproachTags.Analysis, EncounterTypes.Social, AffinityTypes.Natural);

        // Unnatural affinities
        config.SetAffinity(ApproachTags.Dominance, EncounterTypes.Social, AffinityTypes.Unnatural);
        config.SetAffinity(ApproachTags.Concealment, EncounterTypes.Intellectual, AffinityTypes.Unnatural);

        // Dangerous affinities
        config.SetAffinity(ApproachTags.Dominance, EncounterTypes.Physical, AffinityTypes.Dangerous);

        return config;
    }

    public static ArchetypeConfig CreateRanger()
    {
        ArchetypeConfig config = new ArchetypeConfig();
        config.PrimaryApproach = ApproachTags.Precision;

        // Initialize with default neutral values
        config.AffinityValues = new AffinityTypes[Enum.GetValues(typeof(ApproachContexts)).Length];
        for (int i = 0; i < config.AffinityValues.Length; i++)
        {
            config.AffinityValues[i] = AffinityTypes.Neutral;
        }

        // Set ranger-specific affinities

        // Natural affinities
        config.SetAffinity(ApproachTags.Precision, EncounterTypes.Physical, AffinityTypes.Natural);
        config.SetAffinity(ApproachTags.Precision, EncounterTypes.Intellectual, AffinityTypes.Natural);
        config.SetAffinity(ApproachTags.Concealment, EncounterTypes.Physical, AffinityTypes.Natural);

        // Unnatural affinities
        config.SetAffinity(ApproachTags.Dominance, EncounterTypes.Social, AffinityTypes.Unnatural);
        config.SetAffinity(ApproachTags.Rapport, EncounterTypes.Intellectual, AffinityTypes.Unnatural);

        return config;
    }

    public static ArchetypeConfig CreateBard()
    {
        ArchetypeConfig config = new ArchetypeConfig();
        config.PrimaryApproach = ApproachTags.Rapport;

        // Initialize with default neutral values
        config.AffinityValues = new AffinityTypes[Enum.GetValues(typeof(ApproachContexts)).Length];
        for (int i = 0; i < config.AffinityValues.Length; i++)
        {
            config.AffinityValues[i] = AffinityTypes.Neutral;
        }

        // Set bard-specific affinities

        // Natural affinities
        config.SetAffinity(ApproachTags.Rapport, EncounterTypes.Social, AffinityTypes.Natural);
        config.SetAffinity(ApproachTags.Rapport, EncounterTypes.Physical, AffinityTypes.Natural);
        config.SetAffinity(ApproachTags.Analysis, EncounterTypes.Social, AffinityTypes.Natural);

        // Unnatural affinities
        config.SetAffinity(ApproachTags.Concealment, EncounterTypes.Social, AffinityTypes.Unnatural);
        config.SetAffinity(ApproachTags.Dominance, EncounterTypes.Intellectual, AffinityTypes.Unnatural);

        return config;
    }

    public static ArchetypeConfig CreateThief()
    {
        ArchetypeConfig config = new ArchetypeConfig();
        config.PrimaryApproach = ApproachTags.Concealment;

        // Initialize with default neutral values
        config.AffinityValues = new AffinityTypes[Enum.GetValues(typeof(ApproachContexts)).Length];
        for (int i = 0; i < config.AffinityValues.Length; i++)
        {
            config.AffinityValues[i] = AffinityTypes.Neutral;
        }

        // Set thief-specific affinities

        // Natural affinities
        config.SetAffinity(ApproachTags.Concealment, EncounterTypes.Physical, AffinityTypes.Natural);
        config.SetAffinity(ApproachTags.Concealment, EncounterTypes.Social, AffinityTypes.Natural);
        config.SetAffinity(ApproachTags.Precision, EncounterTypes.Physical, AffinityTypes.Natural);

        // Unnatural affinities
        config.SetAffinity(ApproachTags.Dominance, EncounterTypes.Social, AffinityTypes.Unnatural);
        config.SetAffinity(ApproachTags.Rapport, EncounterTypes.Physical, AffinityTypes.Unnatural);

        // Dangerous affinities
        config.SetAffinity(ApproachTags.Dominance, EncounterTypes.Physical, AffinityTypes.Dangerous);

        return config;
    }

    private void SetAffinity(ApproachTags approach, EncounterTypes encounterType, AffinityTypes affinity)
    {
        ApproachContexts context = ConvertToApproachContext(approach, encounterType);
        AffinityValues[(int)context] = affinity;
    }

    public AffinityTypes GetAffinity(ApproachTags approach, EncounterTypes encounterType)
    {
        ApproachContexts context = ConvertToApproachContext(approach, encounterType);
        return AffinityValues[(int)context];
    }

    public List<ApproachTags> GetApproachesWithAffinity(AffinityTypes affinity, EncounterTypes encounterType)
    {
        List<ApproachTags> approaches = new List<ApproachTags>();

        // Check each approach type
        foreach (ApproachTags approach in Enum.GetValues(typeof(ApproachTags)))
        {
            if (IsApproachTag(approach) && GetAffinity(approach, encounterType) == affinity)
            {
                approaches.Add(approach);
            }
        }

        return approaches;
    }

    private bool IsApproachTag(ApproachTags tag)
    {
        return tag == ApproachTags.Dominance ||
               tag == ApproachTags.Rapport ||
               tag == ApproachTags.Analysis ||
               tag == ApproachTags.Precision ||
               tag == ApproachTags.Concealment;
    }

    private ApproachContexts ConvertToApproachContext(ApproachTags approach, EncounterTypes encounterType)
    {
        switch (approach)
        {
            case ApproachTags.Dominance:
                switch (encounterType)
                {
                    case EncounterTypes.Physical: return ApproachContexts.DominancePhysical;
                    case EncounterTypes.Social: return ApproachContexts.DominanceSocial;
                    case EncounterTypes.Intellectual: return ApproachContexts.DominanceIntellectual;
                    default: throw new ArgumentOutOfRangeException(nameof(encounterType));
                }

            case ApproachTags.Rapport:
                switch (encounterType)
                {
                    case EncounterTypes.Physical: return ApproachContexts.RapportPhysical;
                    case EncounterTypes.Social: return ApproachContexts.RapportSocial;
                    case EncounterTypes.Intellectual: return ApproachContexts.RapportIntellectual;
                    default: throw new ArgumentOutOfRangeException(nameof(encounterType));
                }

            case ApproachTags.Analysis:
                switch (encounterType)
                {
                    case EncounterTypes.Physical: return ApproachContexts.AnalysisPhysical;
                    case EncounterTypes.Social: return ApproachContexts.AnalysisSocial;
                    case EncounterTypes.Intellectual: return ApproachContexts.AnalysisIntellectual;
                    default: throw new ArgumentOutOfRangeException(nameof(encounterType));
                }

            case ApproachTags.Precision:
                switch (encounterType)
                {
                    case EncounterTypes.Physical: return ApproachContexts.PrecisionPhysical;
                    case EncounterTypes.Social: return ApproachContexts.PrecisionSocial;
                    case EncounterTypes.Intellectual: return ApproachContexts.PrecisionIntellectual;
                    default: throw new ArgumentOutOfRangeException(nameof(encounterType));
                }

            case ApproachTags.Concealment:
                switch (encounterType)
                {
                    case EncounterTypes.Physical: return ApproachContexts.ConcealmentPhysical;
                    case EncounterTypes.Social: return ApproachContexts.ConcealmentSocial;
                    case EncounterTypes.Intellectual: return ApproachContexts.ConcealmentIntellectual;
                    default: throw new ArgumentOutOfRangeException(nameof(encounterType));
                }

            default:
                throw new ArgumentOutOfRangeException(nameof(approach));
        }
    }
}