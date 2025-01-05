public class LocationSpot
{
    public LocationSpotNames Name;
    public LocationNames LocationName;
    public CharacterNames Character;

    public ActionGenerationContext ActionGenerationContext;
    public List<ActionImplementation> CharacterActions;

    public TimeSlots TimeSlotOpen;

    public LocationSpot(
        LocationSpotNames locationSpotType,
        LocationNames location,
        CharacterNames character,
        ActionGenerationContext actionGenerationContext,
        List<ActionImplementation> characterActions,
        TimeSlots timeSlotOpen
        )
    {
        this.Name = locationSpotType;
        this.LocationName = location;
        this.ActionGenerationContext = actionGenerationContext;
        this.Character = character;
        this.CharacterActions = characterActions;
        this.TimeSlotOpen = timeSlotOpen;
    }
}
