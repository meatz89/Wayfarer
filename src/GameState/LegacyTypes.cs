/// <summary>
/// Temporary types to fix build errors during migration to intent-based architecture
/// These should be removed once all references are updated
/// </summary>

public enum NPCServices
{
    Trade,
    Rest,
    Information,
    Training,
    Healing,
    EquipmentRepair,
    FoodProduction
}

public enum CommandCategory
{
    Movement,
    Social,
    Economic,
    Special,
    Rest
}

public enum EffectType
{
    AddItem,
    RemoveItem,
    AddCoins,
    RemoveCoins,
    AddStamina,
    RemoveStamina,
    SetFlag,
    UnsetFlag
}

public enum ConditionType
{
    HasItem,
    HasCoins,
    HasStamina,
    HasFlag,
    LocationIs,
    TimeIs
}