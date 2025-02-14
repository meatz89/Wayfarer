public class EncounterContext
{
    public ActionImplementation ActionImplementation;
    public Location Location;
    public LocationSpot LocationSpot;
    public BasicActionTypes ActionType;
    public LocationSpotAvailabilityConditions LocationSpotAvailabilityConditions;

    public ChoiceArchetypes LastChoiceType { get; internal set; }
    public ChoiceApproaches LastChoiceApproach { get; internal set; }
    public EncounterChoice LastChoice { get; internal set; }
}
