public class LocationSpot
{
    public LocationSpotNames Name;
    public LocationNames Location;
    public CharacterNames Character;

    public ActionGenerationContext ActionGenerationContext;
    public List<ActionImplementation> CharacterActions;


    public LocationSpot(
        LocationSpotNames locationSpotType,
        LocationNames location,
        CharacterNames character,
        ActionGenerationContext actionGenerationContext,
        List<ActionImplementation> characterActions)
    {
        this.Name = locationSpotType;
        this.Location = location;
        this.ActionGenerationContext = actionGenerationContext;
        this.Character = character;
        this.CharacterActions = characterActions;
    }
}

public class SpaceProperties
{
    public ScaleVariations Scale { get; set; }
    public ExposureConditions Exposure { get; set; }
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
