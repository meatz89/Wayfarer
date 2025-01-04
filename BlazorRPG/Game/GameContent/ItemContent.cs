public class ItemContent
{
    public static Item CharmingPendant => new ItemBuilder()
        .WithName(ItemNames.CharmingPendant)
        .WithDescription("A beautifully crafted pendant that catches everyone's eye")
        .WithActionModifier(actionModifier => actionModifier
            .WithDescription("People are more generous during social interactions")
            .ForActionType(ActionTypes.Mingle)
            .AdditionalCoinReward(1))
        .Build();

    public static Item WoodcuttersAxe => new ItemBuilder()
        .WithName(ItemNames.WoodcuttersAxe)
        .WithDescription("A nice axe for cutting trees")
        .WithActionModifier(actionModifier => actionModifier
            .WithDescription("Gathers more Wood when Gathering")
            .ForActionType(ActionTypes.Gather)
            .WhenResourceRewardHas(ResourceTypes.Wood)
            .AdditionalResourceReward(ResourceTypes.Wood, 1)
            .ReduceActionCost(EnergyTypes.Physical, 1))
        .Build();

    public static Item TorchLight => new ItemBuilder()
        .WithName(ItemNames.Torchlight)
        .WithDescription("A torchlight for nighttime gathering")
        .WithActionModifier(actionModifier => actionModifier
            .WithDescription("Allows gathering at night")
            .ForActionType(ActionTypes.Gather)
            .ForTimeWindow(TimeSlots.Night))
        .WithActionModifier(actionModifier => actionModifier
            .WithDescription("Removes Focus Requirement for gather actions")
            .ForActionType(ActionTypes.Gather)
            .ForTimeWindow(TimeSlots.Night))
        .Build();

    public static Item CraftingApron => new ItemBuilder()
        .WithName(ItemNames.CraftingApron)
        .WithDescription("A sturdy apron that makes crafting easier")
        .WithActionModifier(actionModifier => actionModifier
            .WithDescription("Reduces physical strain when crafting")
            .ForActionType(ActionTypes.Labor)
            .ForLocationType(LocationTypes.Industry)
            .ReduceActionCost(EnergyTypes.Physical, 1))
        .WithActionModifier(actionModifier => actionModifier
            .WithDescription("Allows evening crafting")
            .ForActionType(ActionTypes.Labor)
            .ForLocationType(LocationTypes.Industry)
            .ForTimeWindow(TimeSlots.Evening))
        .Build();
}
