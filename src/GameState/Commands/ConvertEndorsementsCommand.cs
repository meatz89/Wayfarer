using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Command to convert endorsements to seals at guild locations
/// </summary>
public class ConvertEndorsementsCommand : BaseGameCommand
{
    private readonly string _guildLocationId;
    private readonly SealTier _targetTier;
    private readonly EndorsementManager _endorsementManager;
    private readonly MessageSystem _messageSystem;
    private readonly LocationRepository _locationRepository;
    
    public ConvertEndorsementsCommand(
        string guildLocationId,
        SealTier targetTier,
        EndorsementManager endorsementManager,
        MessageSystem messageSystem,
        LocationRepository locationRepository)
    {
        _guildLocationId = guildLocationId;
        _targetTier = targetTier;
        _endorsementManager = endorsementManager;
        _messageSystem = messageSystem;
        _locationRepository = locationRepository;
        
        CommandType = "ConvertEndorsements";
        Description = $"Convert endorsements to {targetTier} seal";
    }
    
    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        var player = gameWorld.GetPlayer();
        
        // Check if player is at the guild location
        var currentLocation = _locationRepository.GetCurrentLocation();
        if (currentLocation?.Id != _guildLocationId)
        {
            return CommandValidationResult.Failure(
                "You must be at the guild to convert endorsements",
                false,
                null);
        }
        
        // Check if conversion is available
        var options = _endorsementManager.GetAvailableSealConversions(_guildLocationId);
        var option = options.FirstOrDefault(o => o.TargetTier == _targetTier);
        
        if (option == null)
        {
            return CommandValidationResult.Failure(
                "This seal tier is not available for conversion",
                false,
                null);
        }
        
        if (!option.CanConvert)
        {
            return CommandValidationResult.Failure(
                $"Need {option.RequiredEndorsements} endorsements, have {option.CurrentEndorsements}",
                true,
                $"Deliver {option.RequiredEndorsements - option.CurrentEndorsements} more endorsements");
        }
        
        return CommandValidationResult.Success();
    }
    
    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        bool success = _endorsementManager.ConvertEndorsementsToSeal(_guildLocationId, _targetTier);
        
        if (success)
        {
            return CommandResult.Success(
                $"Successfully earned {_targetTier} seal!",
                new { SealTier = _targetTier, GuildLocation = _guildLocationId }
            );
        }
        
        return CommandResult.Failure("Failed to convert endorsements");
    }
}