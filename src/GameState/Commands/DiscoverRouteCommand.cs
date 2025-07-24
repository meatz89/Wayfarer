public class DiscoverRouteCommand : BaseGameCommand
{
    private readonly RouteDiscoveryOption _discovery;
    private readonly RouteDiscoveryManager _discoveryManager;
    private readonly MessageSystem _messageSystem;

    public DiscoverRouteCommand(
        RouteDiscoveryOption discovery,
        RouteDiscoveryManager discoveryManager,
        MessageSystem messageSystem)
    {
        _discovery = discovery;
        _discoveryManager = discoveryManager;
        _messageSystem = messageSystem;

        Description = $"Discover route: {_discovery.Route.Name}";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        throw new NotImplementedException();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        // Try to discover the route
        bool success = _discoveryManager.TryDiscoverRoute(_discovery.Route.Id);

        if (success)
        {
            _messageSystem.AddSystemMessage($"Discovered new route: {_discovery.Route.Name}", SystemMessageTypes.Success);
            return CommandResult.Success();
        }
        else
        {
            _messageSystem.AddSystemMessage("Failed to discover route", SystemMessageTypes.Danger);
            return CommandResult.Failure("Discovery requirements not met");
        }
    }
}