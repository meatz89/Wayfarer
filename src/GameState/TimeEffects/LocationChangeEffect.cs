using System.Collections.Generic;


/// <summary>
/// Effect that changes the player's location spot as part of a time-based action.
/// </summary>
public class LocationChangeEffect : ITimeBasedEffect
{
    private readonly Player _player;
    private readonly LocationSpot _newLocationSpot;
    private LocationSpot _previousLocationSpot;

    public LocationChangeEffect(Player player, LocationSpot newLocationSpot)
    {
        _player = player;
        _newLocationSpot = newLocationSpot;
    }

    public EffectValidation Validate(TimeState currentTime, Dictionary<string, object> context)
    {
        if (_newLocationSpot == null)
        {
            return EffectValidation.Invalid("Target location spot is null");
        }

        // Could add more validation here (e.g., checking if location is accessible)
        return EffectValidation.Valid();
    }

    public EffectResult Apply(TimeState currentTime, Dictionary<string, object> context)
    {
        _previousLocationSpot = _player.CurrentLocationSpot;
        _player.CurrentLocationSpot = _newLocationSpot;

        EffectResult result = EffectResult.Succeeded($"Moved to {_newLocationSpot.Name}");
        result.OutputData["previous_location_spot"] = _previousLocationSpot;
        result.OutputData["new_location_spot"] = _newLocationSpot;

        return result;
    }

    public void Rollback(TimeState currentTime, Dictionary<string, object> context)
    {
        _player.CurrentLocationSpot = _previousLocationSpot;
    }
}