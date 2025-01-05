public class LocationSpot
{
    public LocationSpotNames Name;
    public LocationNames Location;
    public CharacterNames Character;

    public ActionImplementation LocationSpotAction;
    public List<ActionImplementation> CharacterActions;

    public AffinityValues affinityValues;

    public LocationSpot(
        LocationSpotNames locationSpotType,
        LocationNames location,
        CharacterNames character,
        ActionImplementation spotAction,
        List<ActionImplementation> characterActions)
    {
        this.Name = locationSpotType;
        this.Location = location;
        this.LocationSpotAction = spotAction;
        this.Character = character;
        this.CharacterActions = characterActions;
    }
}

public enum AffinityValues
{
    Momentum, // values direct action
    Understanding, // values relationships
    Connection, // values analysis
    Tension, // values strategy
    Advantage // values pressure
}

public class SpaceProperties
{
    public AccessTypes Access { get; set; }
    public ScaleVariations Scale { get; set; }
    public ExposureConditions Exposure { get; set; }
    public PopulationDensity Population { get; set; }
}

public class SocialContext
{
    public AuthorityTypes Authority { get; set; }
    public FormalityTypes Formality { get; set; }
    public LegalityTypes Legality { get; set; }
    public TensionState Tension { get; set; }
}

public class ActivityProperties
{
    public ComplexityTypes Complexity { get; set; }
    public DurationTypes Duration { get; set; }
    public IntensityTypes Intensity { get; set; }
    public NoiseTypes Noise { get; set; }
}

// Space Properties define the physical environment
public enum AccessTypes
{
    Public,
    Open
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

public enum PopulationDensity
{
    Busy,
    Empty,
    Sparse,
    Crowded,
}

// Social Context shapes interaction parameters
public enum AuthorityTypes
{
    Official
}

public enum FormalityTypes
{
    Formal,
    Casual
}

public enum LegalityTypes
{
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

public enum DurationTypes
{
    Brief
}

public enum IntensityTypes
{
    Low,
    High,
    Medium
}

public enum NoiseTypes
{
    Quiet
}
