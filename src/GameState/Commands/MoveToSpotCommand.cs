using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Command for traveling between spots within the same location
/// </summary>
public class TravelToSpotCommand : BaseGameCommand
{
    private readonly string _targetSpotId;
    private readonly LocationRepository _locationRepository;
    private readonly LocationSpotRepository _spotRepository;
    private readonly MessageSystem _messageSystem;

    public TravelToSpotCommand(
        string targetSpotId,
        LocationRepository locationRepository,
        LocationSpotRepository spotRepository,
        MessageSystem messageSystem)
    {
        _targetSpotId = targetSpotId;
        _locationRepository = locationRepository;
        _spotRepository = spotRepository;
        _messageSystem = messageSystem;

        CommandType = "Travel";
        Description = $"Move to {targetSpotId}";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        var player = gameWorld.GetPlayer();
        var currentLocation = _locationRepository.GetCurrentLocation();
        var targetSpot = _spotRepository.GetAllLocationSpots().FirstOrDefault(s => s.SpotID == _targetSpotId);

        if (targetSpot == null)
        {
            return CommandValidationResult.Failure("Target spot does not exist");
        }

        if (targetSpot.LocationId != currentLocation.Id)
        {
            return CommandValidationResult.Failure("Target spot is in a different location");
        }

        if (player.CurrentLocationSpot?.SpotID == _targetSpotId)
        {
            return CommandValidationResult.Failure("Already at this spot");
        }

        // Moving between spots in same location costs 1 stamina
        return ValidatePlayerResources(player, 0, 1);
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        var player = gameWorld.GetPlayer();
        var targetSpot = _spotRepository.GetAllLocationSpots().FirstOrDefault(s => s.SpotID == _targetSpotId);

        if (player.Stamina < 1)
        {
            _messageSystem.AddSystemMessage("Not enough stamina to move", SystemMessageTypes.Danger);
            return CommandResult.Failure("Not enough stamina");
        }

        // Spend stamina
        player.SpendStamina(1);

        // Move to new spot
        var currentLocation = _locationRepository.GetCurrentLocation();
        _locationRepository.SetCurrentLocation(currentLocation, targetSpot);

        _messageSystem.AddSystemMessage($"Moved to {targetSpot.Name}", SystemMessageTypes.Success);
        return CommandResult.Success();
    }
}