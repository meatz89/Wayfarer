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
                EnergyTypes.Concentration => RequirementTypes.Concentration,
                _ => RequirementTypes.Other
            },
            HealthRequirement => RequirementTypes.Health,
            ConcentrationRequirement => RequirementTypes.Concentration,
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
                KnowledgeTags.MarketRoutines => RequirementTypes.LocalHistory,
                _ => RequirementTypes.Other
            },
            SkillLevelRequirement skillLevelReq => skillLevelReq.SkillType switch
            {
                SkillTypes.Strength => RequirementTypes.Strength,
                SkillTypes.Perception => RequirementTypes.Perception,
                SkillTypes.Charisma => RequirementTypes.Charisma,
                _ => RequirementTypes.Other
            },
            PlayerNegativeStatusRequirement statusReq => statusReq.Status switch
            {
                PlayerNegativeStatus.Cold => RequirementTypes.Cold,
                PlayerNegativeStatus.Hungry => RequirementTypes.Hungry,
                PlayerNegativeStatus.Injured => RequirementTypes.Injured,
                PlayerNegativeStatus.Exhausted => RequirementTypes.Exhausted,
                PlayerNegativeStatus.Stressed => RequirementTypes.Stressed,
            },
            PlayerReputationRequirement statusReq => statusReq.Reputation switch
            {
                PlayerReputationTypes.Shunned => RequirementTypes.Shunned,
                PlayerReputationTypes.Untrustworthy => RequirementTypes.Untrustworthy,
                PlayerReputationTypes.Neutral => RequirementTypes.Neutral,
                PlayerReputationTypes.Trusted => RequirementTypes.Trusted,
                PlayerReputationTypes.Respected => RequirementTypes.Respected,
                _ => RequirementTypes.Other
            },
            _ => RequirementTypes.Other
        };
    }
}
