public static class RequirementMapper
{
    public static RequirementTypes GetRequirementType(Requirement req)
    {
        return req switch
        {
            EnergyRequirement energyReq => energyReq.EnergyType switch
            {
                EnergyTypes.Physical => RequirementTypes.PhysicalEnergy,
                EnergyTypes.Concentration => RequirementTypes.Concentration,
                _ => RequirementTypes.Other
            },
            HealthRequirement => RequirementTypes.Health,
            FocusRequirement => RequirementTypes.Concentration,
            CoinsRequirement => RequirementTypes.Coins,
            ItemRequirement itemReq => itemReq.ResourceType switch
            {
                ItemTypes.Tool => RequirementTypes.Tool,
                _ => RequirementTypes.Other
            },
            ResourceRequirement resourceReq => resourceReq.ResourceType switch
            {
                ItemTypes.Wood => RequirementTypes.Wood,
                _ => RequirementTypes.Other
            },
            InventorySlotsRequirement => RequirementTypes.InventorySlots,
            KnowledgeRequirement knowledgeReq => knowledgeReq.KnowledgeType switch
            {
                KnowledgeTags.MarketRoutines => RequirementTypes.LocalHistory,
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
            PlayerConfidenceRequirement statusReq => statusReq.Confidence switch
            {
                PlayerConfidenceTypes.Shunned => RequirementTypes.Shunned,
                PlayerConfidenceTypes.Untrustworthy => RequirementTypes.Untrustworthy,
                PlayerConfidenceTypes.Neutral => RequirementTypes.Neutral,
                PlayerConfidenceTypes.Trusted => RequirementTypes.Trusted,
                PlayerConfidenceTypes.Respected => RequirementTypes.Respected,
                _ => RequirementTypes.Other
            },
            _ => RequirementTypes.Other
        };
    }
}
