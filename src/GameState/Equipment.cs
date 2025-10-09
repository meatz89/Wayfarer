public class Equipment : Item
{
    public List<string> EnabledActions { get; set; } = new List<string>();

    public bool EnablesAction(string actionId)
    {
        return EnabledActions.Contains(actionId);
    }

    public static Equipment FromItem(Item item, List<string> enabledActions)
    {
        Equipment equipment = new Equipment
        {
            Id = item.Id,
            Name = item.Name,
            InitiativeCost = item.InitiativeCost,
            BuyPrice = item.BuyPrice,
            SellPrice = item.SellPrice,
            Categories = item.Categories,
            Weight = item.Weight,
            Size = item.Size,
            VenueId = item.VenueId,
            LocationId = item.LocationId,
            Description = item.Description,
            TokenGenerationModifiers = item.TokenGenerationModifiers,
            EnablesTokenGeneration = item.EnablesTokenGeneration,
            IsAvailable = item.IsAvailable,
            EnabledActions = enabledActions ?? new List<string>()
        };

        return equipment;
    }
}
