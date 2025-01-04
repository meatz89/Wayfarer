public static class ActionModifierFactory
{
    public static List<ActionModifier> CreateModifier(ModifierConfiguration config)
    {
        List<ActionModifier> modifiers = new();

        // No modifier conditions specified
        if (config.ActionType == default && config.LocationType == default)
        {
            return modifiers;
        }

        // Create energy cost reducer if configured
        if (config.EnergyReduction > 0)
        {
            modifiers.Add(new EnergyCostReducer(
                config.Description,
                config.Source,
                config.EnergyReduction,
                config.EnergyType,
                config.ActionType));
        }

        // Create conditional resource bonus if configured
        if (config.RequiredResourceReward != default &&
            config.AdditionalResource != default)
        {
            modifiers.Add(new ConditionalResourceBonusModifier(
                config.Description,
                config.Source,
                config.ActionType,
                config.RequiredResourceReward,
                config.AdditionalResource,
                config.AdditionalResourceAmount));
        }

        // Create time slot modifier if configured
        if (config.AdditionalCoins > 0)
        {
            modifiers.Add(new CoinsRewardModifier(
                config.Description,
                config.Source,
                config.AdditionalCoins,
                config.ActionType));
        }

        // Create time slot modifier if configured
        if (config.TimeWindow != default)
        {
            modifiers.Add(new TimeSlotModifier(
                config.Description,
                config.Source,
                config.TimeWindow,
                config.ActionType));
        }

        if (modifiers.Count == 0)
        {
            throw new InvalidOperationException(
                "Invalid modifier configuration - no effects specified");
        }

        return modifiers;
    }
}