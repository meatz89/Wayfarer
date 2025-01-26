public static class RequirementMapper
{
    public static RequirementTypes GetRequirementType(Requirement req)
    {
        return req switch
        {
            PressureRequirement => RequirementTypes.MaxPressure,
            InsightRequirement => RequirementTypes.MinInsight,
            EnergyRequirement energyReq => energyReq.EnergyType switch
            {
                EnergyTypes.Physical => RequirementTypes.PhysicalEnergy,
                EnergyTypes.Focus => RequirementTypes.FocusEnergy,
                EnergyTypes.Social => RequirementTypes.SocialEnergy,
                _ => RequirementTypes.Other
            },
            HealthRequirement => RequirementTypes.Health,
            ConcentrationRequirement => RequirementTypes.Concentration,
            ReputationRequirement => RequirementTypes.Reputation,
            CoinsRequirement => RequirementTypes.Coins,
            SkillRequirement skillReq => skillReq.SkillType switch
            {
                SkillTypes.Strength => RequirementTypes.Strength,
                SkillTypes.Perception => RequirementTypes.Perception,
                SkillTypes.Charisma => RequirementTypes.Charisma,
                _ => RequirementTypes.Other
            },
            ItemRequirement itemReq => itemReq.ResourceType switch
            {
                ItemTypes.Tool => RequirementTypes.Tool,
                _ => RequirementTypes.Other
            },
            ResourceRequirement resourceReq => resourceReq.ResourceType switch
            {
                ResourceTypes.Wood => RequirementTypes.Wood,
                ResourceTypes.Metal => RequirementTypes.Metal,
                _ => RequirementTypes.Other
            },
            InventorySlotsRequirement => RequirementTypes.InventorySlots,
            KnowledgeRequirement knowledgeReq => knowledgeReq.KnowledgeType switch
            {
                KnowledgeTypes.LocalHistory => RequirementTypes.LocalHistory,
                _ => RequirementTypes.Other
            },
            SkillLevelRequirement skillLevelReq => skillLevelReq.SkillType switch
            {
                SkillTypes.Strength => RequirementTypes.Strength,
                SkillTypes.Perception => RequirementTypes.Perception,
                SkillTypes.Charisma => RequirementTypes.Charisma,
                _ => RequirementTypes.Other
            },
            StatusRequirement statusReq => statusReq.Status switch
            {
                PlayerStatus.COLD => RequirementTypes.COLD,
                PlayerStatus.HUNGRY => RequirementTypes.HUNGRY,
                PlayerStatus.INJURED => RequirementTypes.INJURED,
                
                PlayerStatus.EXHAUSTED => RequirementTypes.EXHAUSTED,
                PlayerStatus.STRESSED => RequirementTypes.STRESSED,
                PlayerStatus.SHUNNED => RequirementTypes.SHUNNED,
                PlayerStatus.UNTRUSTWORTHY => RequirementTypes.UNTRUSTWORTHY,
                PlayerStatus.NEUTRAL => RequirementTypes.NEUTRAL,
                PlayerStatus.TRUSTED => RequirementTypes.TRUSTED,
                PlayerStatus.RESPECTED => RequirementTypes.RESPECTED,
                _ => RequirementTypes.Other
            },
            _ => RequirementTypes.Other
        };
    }
}
