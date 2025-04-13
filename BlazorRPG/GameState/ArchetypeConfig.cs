public class ArchetypeConfig
{
    public ApproachAffinities ApproachAffinities = new();
    public FocusAffinities FocusAffinities = new();


    // Static factory method for Warrior archetype
    public static ArchetypeConfig CreateWarrior()
    {
        ArchetypeConfig config = new ArchetypeConfig();

        // Natural affinities
        config.SetAffinity(ApproachTags.Dominance, AffinityTypes.Natural);
        config.SetAffinity(FocusTags.Physical, AffinityTypes.Natural);

        // Dangerous affinities
        config.SetAffinity(ApproachTags.Concealment, AffinityTypes.Incompatible);
        config.SetAffinity(FocusTags.Information, AffinityTypes.Incompatible);

        return config;
    }

    public static ArchetypeConfig CreateScholar()
    {
        ArchetypeConfig config = new ArchetypeConfig();

        // Set scholar-specific affinities

        // Natural affinities
        config.SetAffinity(ApproachTags.Analysis, AffinityTypes.Natural);
        config.SetAffinity(FocusTags.Information, AffinityTypes.Natural);

        // Dangerous affinities
        config.SetAffinity(ApproachTags.Precision, AffinityTypes.Incompatible);
        config.SetAffinity(FocusTags.Resource, AffinityTypes.Incompatible);

        return config;
    }

    public static ArchetypeConfig CreateRanger()
    {
        ArchetypeConfig config = new ArchetypeConfig();

        // Set ranger-specific affinities

        // Natural affinities
        config.SetAffinity(ApproachTags.Precision, AffinityTypes.Natural);
        config.SetAffinity(FocusTags.Resource, AffinityTypes.Natural);

        // Dangerous affinities
        config.SetAffinity(ApproachTags.Rapport, AffinityTypes.Incompatible);
        config.SetAffinity(FocusTags.Physical, AffinityTypes.Incompatible);

        return config;
    }

    public static ArchetypeConfig CreateBard()
    {
        ArchetypeConfig config = new ArchetypeConfig();
        
        // Set bard-specific affinities

        // Natural affinities
        config.SetAffinity(ApproachTags.Rapport, AffinityTypes.Natural);
        config.SetAffinity(FocusTags.Relationship, AffinityTypes.Natural);

        // Dangerous affinities
        config.SetAffinity(ApproachTags.Analysis, AffinityTypes.Incompatible);
        config.SetAffinity(FocusTags.Environment, AffinityTypes.Incompatible);

        return config;
    }

    public static ArchetypeConfig CreateThief()
    {
        ArchetypeConfig config = new ArchetypeConfig();

        // Set thief-specific affinities

        // Natural affinities
        config.SetAffinity(ApproachTags.Concealment, AffinityTypes.Natural);
        config.SetAffinity(FocusTags.Environment, AffinityTypes.Natural);

        // Dangerous affinities
        config.SetAffinity(ApproachTags.Dominance, AffinityTypes.Incompatible);
        config.SetAffinity(FocusTags.Relationship, AffinityTypes.Incompatible);

        return config;
    }


    private void SetAffinity(ApproachTags approach, AffinityTypes affinity)
    {
        switch (approach)
        {
            case ApproachTags.Dominance:
                ApproachAffinities.Dominance = affinity;
                break;
            case ApproachTags.Precision:
                ApproachAffinities.Precision = affinity;
                break;
            case ApproachTags.Analysis:
                ApproachAffinities.Analysis = affinity;
                break;
            case ApproachTags.Rapport:
                ApproachAffinities.Rapport = affinity;
                break;
            case ApproachTags.Concealment:
                ApproachAffinities.Concealment = affinity;
                break;
        }
    }

    public AffinityTypes GetAffinity(ApproachTags approach)
    {
        switch (approach)
        {
            case ApproachTags.Dominance:
                return ApproachAffinities.Dominance;

            case ApproachTags.Precision:
                return ApproachAffinities.Precision;

            case ApproachTags.Analysis:
                return ApproachAffinities.Analysis;

            case ApproachTags.Rapport:
                return ApproachAffinities.Rapport;

            case ApproachTags.Concealment:
                return ApproachAffinities.Concealment;
        }
        return AffinityTypes.Compatible;
    }

    private void SetAffinity(FocusTags focus, AffinityTypes affinity)
    {
        switch (focus)
        {
            case FocusTags.Relationship:
                FocusAffinities.Relationship = affinity;
                break;
            case FocusTags.Information:
                FocusAffinities.Information = affinity;
                break;
            case FocusTags.Physical:
                FocusAffinities.Physical = affinity;
                break;
            case FocusTags.Environment:
                FocusAffinities.Environment = affinity;
                break;
            case FocusTags.Resource:
                FocusAffinities.Resource = affinity;
                break;
        }
    }

    public AffinityTypes GetAffinity(FocusTags focus)
    {
        switch (focus)
        {
            case FocusTags.Relationship:
                return FocusAffinities.Relationship;

            case FocusTags.Information:
                return FocusAffinities.Information;

            case FocusTags.Physical:
                return FocusAffinities.Physical;

            case FocusTags.Environment:
                return FocusAffinities.Environment;

            case FocusTags.Resource:
                return FocusAffinities.Resource;
        }
        return AffinityTypes.Compatible;
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

    public List<FocusTags> GetFocusesWithAffinity(AffinityTypes affinity)
    {
        List<FocusTags> focuses = new List<FocusTags>();

        // Check each approach type
        foreach (FocusTags focus in Enum.GetValues(typeof(FocusTags)))
        {
            if (GetAffinity(focus) == affinity)
            {
                focuses.Add(focus);
            }
        }

        return focuses;
    }
}