public class ApproachDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public CardTypes RequiredCardType { get; set; }
    public SkillTypes PrimarySkill { get; set; }
    public SkillTypes SecondarySkill { get; set; }

    public List<string> ContextTags { get; set; } = new List<string>();
    public List<string> ApproachTags { get; set; } = new List<string>();
    public List<string> DomainTags { get; set; } = new List<string>();
}