public class ApproachDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public SkillCategories RequiredCardType { get; set; }
    public SkillTypes PrimarySkill { get; set; }
    public SkillTypes SecondarySkill { get; set; }
}