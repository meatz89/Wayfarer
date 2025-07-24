public class TravelCommand : BaseGameCommand
{
    private readonly RouteOption _route;
    private readonly TravelManager _travelManager;
    private readonly MessageSystem _messageSystem;

    public TravelCommand(
        RouteOption route,
        TravelManager travelManager,
        MessageSystem messageSystem)
    {
        _route = route;
        _travelManager = travelManager;
        _messageSystem = messageSystem;

        CommandType = "Travel";
        Description = $"Travel via {_route.Name}";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();

        // Calculate costs
        int staminaCost = _travelManager.CalculateStaminaCost(_route);
        int coinCost = _travelManager.CalculateCoinCost(_route);

        // Use base class validation helper
        return ValidatePlayerResources(player, coinCost, staminaCost);
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();

        // Check if player can afford the travel
        int staminaCost = _travelManager.CalculateStaminaCost(_route);
        int coinCost = _travelManager.CalculateCoinCost(_route);

        if (player.Stamina < staminaCost)
        {
            _messageSystem.AddSystemMessage($"Not enough stamina. Need {staminaCost}, have {player.Stamina}", SystemMessageTypes.Danger);
            return CommandResult.Failure("Not enough stamina");
        }

        if (player.Coins < coinCost)
        {
            _messageSystem.AddSystemMessage($"Not enough coins. Need {coinCost}, have {player.Coins}", SystemMessageTypes.Danger);
            return CommandResult.Failure("Not enough coins");
        }

        // Apply costs
        player.SpendStamina(staminaCost);
        player.ModifyCoins(-coinCost);

        // Perform travel
        _travelManager.TravelToLocation(_route);

        _messageSystem.AddSystemMessage($"Traveled to {_route.Destination} via {_route.Name}", SystemMessageTypes.Success);
        return CommandResult.Success();
    }

}