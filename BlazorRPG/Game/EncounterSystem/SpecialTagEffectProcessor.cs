
/// <summary>
/// Special effects that need to be processed at encounter level
/// </summary>
public class SpecialTagEffectProcessor
{
    /// <summary>
    /// Process special tag effects after standard effects
    /// </summary>
    public void ProcessSpecialTagEffects(List<EncounterTag> activeTags, EncounterState state, ChoiceProjection choiceProjection)
    {
        foreach (EncounterTag tag in activeTags)
        {
            if (tag.Effect.IsSpecialEffect)
            {
                switch (tag.Effect.SpecialEffectId)
                {
                    case "convert_pressure_to_momentum":
                        ProcessConvertPressureToMomentum(state, choiceProjection);
                        break;

                    case "reduce_pressure_each_turn":
                        ProcessReducePressureEachTurn(state, tag.Effect.PressureModifier);
                        break;

                    case "additional_turn_no_pressure":
                        ProcessAdditionalTurnNoPressure(state);
                        break;

                    case "encounter_auto_success":
                        ProcessEncounterAutoSuccess(state);
                        break;
                }
            }
        }
    }

    private void ProcessConvertPressureToMomentum(EncounterState state, ChoiceProjection choiceProjection)
    {
        state.Momentum += choiceProjection.PressureChange;
        state.Pressure -= choiceProjection.PressureChange;
    }

    private void ProcessReducePressureEachTurn(EncounterState state, int amount)
    {
        if (state.Pressure > 0)
        {
            state.Pressure = Math.Max(0, state.Pressure + amount);
        }
    }

    private void ProcessAdditionalTurnNoPressure(EncounterState state)
    {
        // For the extra turn effect, we would need to integrate with the turn system
        // For now, just zero pressure
        state.Pressure = 0;
    }

    private void ProcessEncounterAutoSuccess(EncounterState state)
    {
        state.Momentum = Math.Max(state.Momentum, 15);
    }
}