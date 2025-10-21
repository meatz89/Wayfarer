namespace Wayfarer.GameState.Enums
{
    /// <summary>
    /// Categorical durability property for Exhaustible equipment
    /// Parser translates to mechanical values (uses, repair cost)
    /// </summary>
    public enum DurabilityType
    {
        Fragile,    // 2 uses, 10 coins repair (delicate tools, simple rope)
        Sturdy,     // 5 uses, 25 coins repair (quality tools, climbing gear)
        Durable     // 8 uses, 40 coins repair (masterwork equipment, reinforced gear)
    }
}
