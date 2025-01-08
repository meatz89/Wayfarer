public class EncounterActionContext
{
    public BasicActionTypes ActionType { get; }
    public LocationTypes LocationType { get; }
    public LocationArchetype LocationArchetype { get; }
    public TimeSlots TimeSlot { get; }
    public LocationProperties LocationProperties { get; }
    public PlayerState PlayerState { get; }
    public EncounterStateValues CurrentValues { get; }
    public int StageNumber { get; } // Add this field

    public EncounterActionContext(
        BasicActionTypes actionType,
        LocationTypes locationType,
        LocationArchetype locationarcheType,
        TimeSlots timeSlot,
        LocationProperties locationProperties,
        PlayerState playerState,
        EncounterStateValues currentValues,
        int stageNumber)
    {
        ActionType = actionType;
        LocationType = locationType;
        LocationArchetype = locationarcheType;
        TimeSlot = timeSlot;
        LocationProperties = locationProperties;
        PlayerState = playerState;
        CurrentValues = currentValues;

        StageNumber = stageNumber;  // Set the field
    }
}
