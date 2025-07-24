using System;
using System.Threading.Tasks;

/// <summary>
/// Command to rest and recover stamina
/// </summary>
public class RestCommand : BaseGameCommand
{
    private readonly string _locationSpotId;
    private readonly int _hours;
    private readonly int _staminaRecovery;
    private readonly MessageSystem _messageSystem;


    public RestCommand(
        string locationSpotId,
        int hours,
        int staminaRecovery,
        MessageSystem messageSystem)
    {
        _locationSpotId = locationSpotId ?? throw new ArgumentNullException(nameof(locationSpotId));
        _hours = hours;
        _staminaRecovery = staminaRecovery;
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));

        Description = $"Rest for {hours} hour(s) to recover {staminaRecovery} stamina";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();

        // Validate location
        if (player.CurrentLocationSpot?.SpotID != _locationSpotId)
        {
            return CommandValidationResult.Failure("Not at the specified location");
        }

        // Check if already at max stamina
        if (player.Stamina >= player.MaxStamina)
        {
            return CommandValidationResult.Failure("Already at maximum stamina");
        }

        // Time cost check removed - handled by executing service

        return CommandValidationResult.Success();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();

        // Calculate actual recovery (capped at max)
        int currentStamina = player.Stamina;
        int potentialRecovery = Math.Min(_staminaRecovery, player.MaxStamina - currentStamina);

        // Time spending handled by executing service

        // Apply stamina recovery
        player.ModifyStamina(potentialRecovery);

        _messageSystem.AddSystemMessage(
            $"Rested for {_hours} hour(s) and recovered {potentialRecovery} stamina",
            SystemMessageTypes.Success
        );

        return CommandResult.Success(
            "Rest completed",
            new
            {
                TimeCost = _hours,  // Use TimeCost instead of HoursSpent
                StaminaRecovered = potentialRecovery,
                CurrentStamina = player.Stamina,
                MaxStamina = player.MaxStamina
            }
        );
    }

}