public static class SkillTagMappings
{
    public static List<SkillApproachMapping> ApproachMappings = new List<SkillApproachMapping>
    {
        new SkillApproachMapping { SkillType = SkillTypes.Subterfuge, ApproachTag = ApproachTags.Analysis, Multiplier = 50 },
        new SkillApproachMapping { SkillType = SkillTypes.Subterfuge, ApproachTag = ApproachTags.Precision, Multiplier = 30 },
        new SkillApproachMapping { SkillType = SkillTypes.Warfare, ApproachTag = ApproachTags.Precision, Multiplier = 40 },
        new SkillApproachMapping { SkillType = SkillTypes.Warfare, ApproachTag = ApproachTags.Concealment, Multiplier = 20 },
        new SkillApproachMapping { SkillType = SkillTypes.Wilderness, ApproachTag = ApproachTags.Analysis, Multiplier = 40 },
        new SkillApproachMapping { SkillType = SkillTypes.Wilderness, ApproachTag = ApproachTags.Precision, Multiplier = 40 },
        new SkillApproachMapping { SkillType = SkillTypes.Scholarship, ApproachTag = ApproachTags.Analysis, Multiplier = 40 },
        new SkillApproachMapping { SkillType = SkillTypes.Scholarship, ApproachTag = ApproachTags.Concealment, Multiplier = 30 },
        new SkillApproachMapping { SkillType = SkillTypes.Diplomacy, ApproachTag = ApproachTags.Rapport, Multiplier = 50 },
        new SkillApproachMapping { SkillType = SkillTypes.Diplomacy, ApproachTag = ApproachTags.Dominance, Multiplier = 20 }
    };

    public static List<SkillFocusMapping> FocusMappings = new List<SkillFocusMapping>
    {
        new SkillFocusMapping { SkillType = SkillTypes.Subterfuge, FocusTag = FocusTags.Environment, Multiplier = 0.4f },
        new SkillFocusMapping { SkillType = SkillTypes.Warfare, FocusTag = FocusTags.Resource, Multiplier = 0.5f },
        new SkillFocusMapping { SkillType = SkillTypes.Warfare, FocusTag = FocusTags.Environment, Multiplier = 0.3f },
        new SkillFocusMapping { SkillType = SkillTypes.Wilderness, FocusTag = FocusTags.Resource, Multiplier = 0.4f },
        new SkillFocusMapping { SkillType = SkillTypes.Wilderness, FocusTag = FocusTags.Information, Multiplier = 0.3f },
        new SkillFocusMapping { SkillType = SkillTypes.Scholarship, FocusTag = FocusTags.Information, Multiplier = 0.4f },
        new SkillFocusMapping { SkillType = SkillTypes.Scholarship, FocusTag = FocusTags.Environment, Multiplier = 0.3f },
        new SkillFocusMapping { SkillType = SkillTypes.Diplomacy, FocusTag = FocusTags.Relationship, Multiplier = 0.5f }
    };
}