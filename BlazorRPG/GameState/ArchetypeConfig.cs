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
        config.AffinityValues = new AffinityTypes[Enum.GetValues(typeof(ApproachTags)).Length];
        for (int i = 0; i < config.AffinityValues.Length; i++)
        {
            config.AffinityValues[i] = AffinityTypes.Neutral;
        }

        // Set warrior-specific affinities

        // Natural affinities
        config.SetAffinity(ApproachTags.Dominance, AffinityTypes.Natural);

        // Unnatural affinities
        config.SetAffinity(ApproachTags.Analysis, AffinityTypes.Unnatural);

        // Dangerous affinities
        config.SetAffinity(ApproachTags.Concealment, AffinityTypes.Dangerous);

        return config;
    }

    public static ArchetypeConfig CreateScholar()
    {
        ArchetypeConfig config = new ArchetypeConfig();
        config.PrimaryApproach = ApproachTags.Analysis;

        // Initialize with default neutral values
        config.AffinityValues = new AffinityTypes[Enum.GetValues(typeof(ApproachTags)).Length];
        for (int i = 0; i < config.AffinityValues.Length; i++)
        {
            config.AffinityValues[i] = AffinityTypes.Neutral;
        }

        // Set scholar-specific affinities

        // Natural affinities
        config.SetAffinity(ApproachTags.Analysis, AffinityTypes.Natural);

        // Unnatural affinities
        config.SetAffinity(ApproachTags.Concealment, AffinityTypes.Unnatural);

        // Dangerous affinities
        config.SetAffinity(ApproachTags.Dominance, AffinityTypes.Dangerous);

        return config;
    }

    public static ArchetypeConfig CreateRanger()
    {
        ArchetypeConfig config = new ArchetypeConfig();
        config.PrimaryApproach = ApproachTags.Precision;

        // Initialize with default neutral values
        config.AffinityValues = new AffinityTypes[Enum.GetValues(typeof(ApproachTags)).Length];
        for (int i = 0; i < config.AffinityValues.Length; i++)
        {
            config.AffinityValues[i] = AffinityTypes.Neutral;
        }

        // Set ranger-specific affinities

        // Natural affinities
        config.SetAffinity(ApproachTags.Precision, AffinityTypes.Natural);

        // Unnatural affinities
        config.SetAffinity(ApproachTags.Dominance, AffinityTypes.Unnatural);

        // Dangerous affinities
        config.SetAffinity(ApproachTags.Precision, AffinityTypes.Dangerous);

        return config;
    }

    public static ArchetypeConfig CreateBard()
    {
        ArchetypeConfig config = new ArchetypeConfig();
        config.PrimaryApproach = ApproachTags.Rapport;

        // Initialize with default neutral values
        config.AffinityValues = new AffinityTypes[Enum.GetValues(typeof(ApproachTags)).Length];
        for (int i = 0; i < config.AffinityValues.Length; i++)
        {
            config.AffinityValues[i] = AffinityTypes.Neutral;
        }

        // Set bard-specific affinities

        // Natural affinities
        config.SetAffinity(ApproachTags.Rapport, AffinityTypes.Natural);

        // Unnatural affinities
        config.SetAffinity(ApproachTags.Dominance, AffinityTypes.Unnatural);

        // Dangerous affinities
        config.SetAffinity(ApproachTags.Analysis, AffinityTypes.Dangerous);

        return config;
    }

    public static ArchetypeConfig CreateThief()
    {
        ArchetypeConfig config = new ArchetypeConfig();
        config.PrimaryApproach = ApproachTags.Concealment;

        // Initialize with default neutral values
        config.AffinityValues = new AffinityTypes[Enum.GetValues(typeof(ApproachTags)).Length];
        for (int i = 0; i < config.AffinityValues.Length; i++)
        {
            config.AffinityValues[i] = AffinityTypes.Neutral;
        }

        // Set thief-specific affinities

        // Natural affinities
        config.SetAffinity(ApproachTags.Concealment, AffinityTypes.Natural);

        // Unnatural affinities
        config.SetAffinity(ApproachTags.Dominance, AffinityTypes.Unnatural);

        // Dangerous affinities
        config.SetAffinity(ApproachTags.Rapport, AffinityTypes.Dangerous);

        return config;
    }

    private void SetAffinity(ApproachTags approach, AffinityTypes affinity)
    {
        AffinityValues[(int)approach] = affinity;
    }

    public AffinityTypes GetAffinity(ApproachTags approach)
    {
        return AffinityValues[(int)approach];
    }

    public List<ApproachTags> GetApproachesWithAffinity(AffinityTypes affinity)
    {
        List<ApproachTags> approaches = new List<ApproachTags>();

        // Check each approach type
        foreach (ApproachTags approach in Enum.GetValues(typeof(ApproachTags)))
        {
            if (GetAffinity(approach) == affinity)
            {
                approaches.Add(approach);
            }
        }

        return approaches;
    }
}