public class EncounterActionContext
{
    public BasicActionTypes ActionType { get; set; }
    public TimeSlots TimeSlot { get; set; }
    public LocationTypes LocationType { get; set; }
    public LocationNames LocationName { get; set; }

    // Character Content
    public CharacterNames CharacterName { get; set; }
    public CharacterRoleTypes Role { get; set; }
    public RelationshipStatus Relationship { get; set; }
    public EncounterStateValues CurrentValues { get; internal set; }
}
