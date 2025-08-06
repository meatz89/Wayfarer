using System.Collections.Generic;


/// <summary>
/// Effect that deducts stamina as part of a time-based action.
/// </summary>
public class StaminaCostEffect : ITimeBasedEffect
{
    private readonly Player _player;
    private readonly int _staminaCost;
    private int _previousStamina;

    public StaminaCostEffect(Player player, int staminaCost)
    {
        _player = player;
        _staminaCost = staminaCost;
    }

    public EffectValidation Validate(TimeState currentTime, Dictionary<string, object> context)
    {
        if (_player.Stamina < _staminaCost)
        {
            return EffectValidation.Invalid($"Insufficient stamina. Need {_staminaCost}, have {_player.Stamina}");
        }

        return EffectValidation.Valid();
    }

    public EffectResult Apply(TimeState currentTime, Dictionary<string, object> context)
    {
        _previousStamina = _player.Stamina;
        _player.Stamina -= _staminaCost;

        return EffectResult.Succeeded($"Spent {_staminaCost} stamina");
    }

    public void Rollback(TimeState currentTime, Dictionary<string, object> context)
    {
        _player.Stamina = _previousStamina;
    }
}