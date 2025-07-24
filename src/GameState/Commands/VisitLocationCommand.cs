/// <summary>
/// Command to mark a location as visited and update related state.
/// </summary>
public class VisitLocationCommand : BaseGameCommand
{
    private readonly string _locationId;

    public VisitLocationCommand(string locationId)
    {
        if (string.IsNullOrWhiteSpace(locationId))
            throw new ArgumentException("Location ID cannot be empty", nameof(locationId));

        _locationId = locationId;

        Description = $"Visit location '{_locationId}'";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        // Find the location
        Location? location = gameWorld.Locations.FirstOrDefault(l => l.Id == _locationId);
        if (location == null)
        {
            return CommandValidationResult.Failure(
                $"Location '{_locationId}' not found",
                canBeRemedied: false
            );
        }

        // Check if player can access the location
        ExtendedPlayerState playerState = ExtendedPlayerState.FromPlayer(gameWorld.GetPlayer());
        LocationState locationState = LocationState.FromLocation(location);

        if (!LocationStateOperations.IsAccessible(locationState, playerState, gameWorld.CurrentDay))
        {
            return CommandValidationResult.Failure(
                $"Cannot access {location.Name} - requirements not met",
                canBeRemedied: true,
                remediationHint: "Check access requirements for this location"
            );
        }

        return CommandValidationResult.Success();
    }

    public override Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        CommandValidationResult validation = CanExecute(gameWorld);
        if (!validation.IsValid)
        {
            return Task.FromResult(CommandResult.Failure(validation.FailureReason));
        }

        // Find and update the location
        Location location = gameWorld.Locations.First(l => l.Id == _locationId);
        LocationState previousLocationState = LocationState.FromLocation(location);
        bool wasAlreadyVisited = location.HasBeenVisited;

        // Apply the visit
        LocationOperationResult result = LocationStateOperations.VisitLocation(previousLocationState);
        if (!result.IsSuccess)
        {
            return Task.FromResult(CommandResult.Failure(result.Message));
        }

        // Update the mutable location (temporary until full immutability)
        location.HasBeenVisited = true;
        location.VisitCount++;

        // Update player knowledge
        Player player = gameWorld.GetPlayer();
        if (!player.DiscoveredLocationIds.Contains(_locationId))
        {
            player.DiscoveredLocationIds.Add(_locationId);
        }
        if (!player.KnownLocations.Contains(_locationId))
        {
            player.KnownLocations.Add(_locationId);
        }

        // Add discovery message if first visit
        if (!wasAlreadyVisited)
        {
            gameWorld.SystemMessages.Add(new SystemMessage($"Discovered new location: {location.Name}"));
        }

        // Add visit message
        gameWorld.SystemMessages.Add(new SystemMessage(result.Message));

        return Task.FromResult(CommandResult.Success(
            result.Message,
            new
            {
                LocationId = _locationId,
                LocationName = location.Name,
                VisitCount = location.VisitCount,
                FirstVisit = !wasAlreadyVisited
            }
        ));
    }

}