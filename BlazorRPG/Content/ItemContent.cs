public class ItemContent
{
    public static Item CharmingPendant => new ItemBuilder()
        .WithName(ItemNames.CharmingPendant)
        .WithDescription("A beautifully crafted pendant that catches everyone's eye")
        .WithActionModifier((Action<ActionModifierBuilder>)(actionModifier => actionModifier
            .WithDescription("People are more generous during social interactions")
            .ForActionType((BasicActionTypes)BasicActionTypes.Discuss)
            .AdditionalCoinReward(1)))
        .Build();

    public static Item WoodcuttersAxe => new ItemBuilder()
        .WithName(ItemNames.WoodcuttersAxe)
        .WithDescription("A nice axe for cutting trees")
        .WithActionModifier((Action<ActionModifierBuilder>)(actionModifier => actionModifier
            .WithDescription("GATHERs more Wood when GATHERing")
            .ForActionType((BasicActionTypes)BasicActionTypes.Gather)
            .WhenResourceRewardHas(ResourceTypes.Wood)
            .AdditionalResourceReward(ResourceTypes.Wood, 1)
            .ReduceActionCost(EnergyTypes.Physical, 1)))
        .Build();

    public static Item TorchLight => new ItemBuilder()
        .WithName(ItemNames.Torchlight)
        .WithDescription("A torchlight for nighttime gathering")
        .WithActionModifier((Action<ActionModifierBuilder>)(actionModifier => actionModifier
            .WithDescription("Allows gathering at night")
            .ForActionType((BasicActionTypes)BasicActionTypes.Gather)
            .ForTimeWindow(TimeWindows.Night)))
        .WithActionModifier((Action<ActionModifierBuilder>)(actionModifier => actionModifier
            .WithDescription("Removes Focus Requirement for gather actions")
            .ForActionType((BasicActionTypes)BasicActionTypes.Gather)
            .ForTimeWindow(TimeWindows.Night)))
        .Build();

    public static Item CraftingApron => new ItemBuilder()
        .WithName(ItemNames.CraftingApron)
        .WithDescription("A sturdy apron that makes crafting easier")
        .WithActionModifier(actionModifier => actionModifier
            .WithDescription("Reduces physical strain when crafting")
            .ForActionType(BasicActionTypes.Labor)
            .ForLocationType(LocationTypes.Industrial)
            .ReduceActionCost(EnergyTypes.Physical, 1))
        .WithActionModifier(actionModifier => actionModifier
            .WithDescription("Allows evening crafting")
            .ForActionType(BasicActionTypes.Labor)
            .ForLocationType(LocationTypes.Industrial)
            .ForTimeWindow(TimeWindows.Evening))
        .Build();
}
