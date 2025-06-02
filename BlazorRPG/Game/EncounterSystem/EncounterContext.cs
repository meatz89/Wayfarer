public class EncounterContext
{
    public GameWorld gameWorld;

    public string LocationName { get; set; }
    public string LocationSpotName { get; set; }
    public string ActionName { get; set; }
    public SkillCategories SkillCategory { get; set; }
    public List<SkillCard> PlayerSkillCards { get; set; }
    public List<SkillCard> PlayerAllCards { get; set; }

    public NPC TargetNPC { get; set; }
    public List<string> LocationProperties { get; set; }

    public string LocationDescription { get; set; }
    public string ObjectiveDescription { get; set; }
    public LocationAction LocationAction { get; set; }
    public int StartingFocusPoints { get; set; }
    public Player Player { get; set; }
    public ApproachDefinition? ActionApproach { get; set; }
}
