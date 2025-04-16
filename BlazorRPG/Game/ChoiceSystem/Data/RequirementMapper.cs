public static class RequirementMapper
{
    public static RequirementTypes GetRequirementType(Requirement req)
    {
        return req switch
        {
            EnergyRequirement => RequirementTypes.Energy,
            HealthRequirement => RequirementTypes.Health,
            CoinsRequirement => RequirementTypes.Coins,

            InventorySlotsRequirement => RequirementTypes.InventorySlots,
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
