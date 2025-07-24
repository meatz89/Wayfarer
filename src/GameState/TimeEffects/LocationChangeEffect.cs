using System.Collections.Generic;


/// <summary>
/// Effect that changes the player's location as part of a time-based action.
/// </summary>
public class LocationChangeEffect : ITimeBasedEffect
{
    private readonly Player _player;
    private readonly Location _newLocation;
    private Location _previousLocation;

    public LocationChangeEffect(Player player, Location newLocation)
    {
        _player = player;
        _newLocation = newLocation;
    }

    public EffectValidation Validate(TimeState currentTime, Dictionary<string, object> context)
    {
        if (_newLocation == null)
        {
            return EffectValidation.Invalid("Target location is null");
        }

        // Could add more validation here (e.g., checking if location is accessible)
        return EffectValidation.Valid();
    }

    public EffectResult Apply(TimeState currentTime, Dictionary<string, object> context)
    {
        _previousLocation = _player.CurrentLocation;
        _player.CurrentLocation = _newLocation;

        EffectResult result = EffectResult.Succeeded($"Moved to {_newLocation.Name}");
        result.OutputData["previous_location"] = _previousLocation;
        result.OutputData["new_location"] = _newLocation;

        return result;
    }

    public void Rollback(TimeState currentTime, Dictionary<string, object> context)
    {
        _player.CurrentLocation = _previousLocation;
    }
}