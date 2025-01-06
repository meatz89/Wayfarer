public enum BasicActionTypes
{
    // Physical Actions define direct interaction with the world:
    Labor, // for directed physical effort
    Gather, // for collecting and taking
    Craft, // for creating and combining
    Move, // for traversing and positioning

    // Social Actions handle character interactions:
    Mingle, // for casual interaction
    Trade, // for formal exchange
    Persuade, // for directed influence
    Perform, // for entertainment and display

    // Mental Actions cover intellectual activities:
    Investigate, // for directed observation
    Study, // for focused learning
    Plan, // for strategic thinking
    Reflect, // for processing and rest
    Rest,
    Wait,
}

public class SpaceProperties
{
    public ScaleVariations Scale { get; set; }
    public ExposureConditions Exposure { get; set; }
    public CrowdLevel CrowdLevel { get; internal set; }
}

public class SocialContext
{
    public LegalityTypes Legality { get; set; }
    public TensionState Tension { get; set; }
}

public class ActivityProperties
{
    public ComplexityTypes Complexity { get; set; }
}

public enum ScaleVariations
{
    Medium,
    Intimate,
    Large
}

public enum ExposureConditions
{
    Indoor,
    Outdoor,
}

public enum CrowdLevel
{
    Empty,
    Sparse,
    Populated,
    Busy
}

public enum LegalityTypes
{
    Legal,
    Illegal
}

public enum TensionState
{
    Relaxed,
    Alert,
    Hostile,
}

// Activity Properties define execution requirements
public enum ComplexityTypes
{
    Complex,
    Simple
}

