public static class SkillTagMappings
{
    public static List<SkillApproachMapping> ApproachMappings = new List<SkillApproachMapping>
    {
        new SkillApproachMapping(ApproachTags.Dominance, SkillTypes.Endurance, 0.2f),
        new SkillApproachMapping(ApproachTags.Dominance, SkillTypes.Diplomacy, 0.2f),
        new SkillApproachMapping(ApproachTags.Rapport, SkillTypes.Charm, 0.2f),
        new SkillApproachMapping(ApproachTags.Rapport, SkillTypes.Diplomacy, 0.2f),
        new SkillApproachMapping(ApproachTags.Analysis, SkillTypes.Lore, 0.2f),
        new SkillApproachMapping(ApproachTags.Analysis, SkillTypes.Insight, 0.2f),
        new SkillApproachMapping(ApproachTags.Precision, SkillTypes.Finesse, 0.2f),
        new SkillApproachMapping(ApproachTags.Precision, SkillTypes.Insight, 0.1f),
        new SkillApproachMapping(ApproachTags.Concealment, SkillTypes.Finesse, 0.2f),
        new SkillApproachMapping(ApproachTags.Concealment, SkillTypes.Insight, 0.2f),
        new SkillApproachMapping(ApproachTags.None, SkillTypes.Endurance, 0f),
        new SkillApproachMapping(ApproachTags.None, SkillTypes.Finesse, 0f),
        new SkillApproachMapping(ApproachTags.None, SkillTypes.Diplomacy, 0f),
        new SkillApproachMapping(ApproachTags.None, SkillTypes.Charm, 0f),
        new SkillApproachMapping(ApproachTags.None, SkillTypes.Insight, 0f),
        new SkillApproachMapping(ApproachTags.None, SkillTypes.Lore, 0f)
    };

    public static List<SkillFocusMapping> FocusMappings = new List<SkillFocusMapping>
    {
        new SkillFocusMapping(FocusTags.Relationship, SkillTypes.Charm, 0.2f),
        new SkillFocusMapping(FocusTags.Relationship, SkillTypes.Diplomacy, 0.2f),
        new SkillFocusMapping(FocusTags.Information, SkillTypes.Lore, 0.2f),
        new SkillFocusMapping(FocusTags.Information, SkillTypes.Insight, 0.2f),
        new SkillFocusMapping(FocusTags.Physical, SkillTypes.Endurance, 0.2f),
        new SkillFocusMapping(FocusTags.Physical, SkillTypes.Finesse, 0.2f),
        new SkillFocusMapping(FocusTags.Environment, SkillTypes.Insight, 0.2f),
        new SkillFocusMapping(FocusTags.Environment, SkillTypes.Endurance, 0.1f),
        new SkillFocusMapping(FocusTags.Resource, SkillTypes.Diplomacy, 0.2f),
        new SkillFocusMapping(FocusTags.Resource, SkillTypes.Lore, 0.1f)
    };
}
