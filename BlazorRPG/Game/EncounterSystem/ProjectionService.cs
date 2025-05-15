
public class ProjectionService
{
    private readonly Encounter _encounterInfo;
    private readonly PlayerState _playerState;

    public ProjectionService(
        Encounter encounterInfo,
        PlayerState playerState)
    {
        _encounterInfo = encounterInfo;
        _playerState = playerState;
    }

    public ChoiceProjection CreateChoiceProjection(
        CardDefinition choice,
        int currentMomentum,
        int currentPressure,
        int currentTurn)
    {
        ChoiceProjection projection = new ChoiceProjection(choice);

        int momentumChange = 0;
        int pressureChange = 0;

        // 1. Calculate base card effect
        CalculateBaseCardEffect(choice, projection, ref momentumChange, ref pressureChange);

        // 2. Calculate environmental turn pressure
        CalculateEnvironmentalPressure(currentTurn, projection, ref pressureChange);

        CalculateSkillBonuses(choice, projection, ref momentumChange, ref pressureChange);

        // Ensure values don't go negative
        EnsureNoNegativeValues(currentMomentum, currentPressure, projection, ref momentumChange, ref pressureChange);

        // Set final projection values
        projection.MomentumGained = momentumChange;
        projection.PressureBuilt = pressureChange;
        projection.FinalMomentum = currentMomentum + momentumChange;
        projection.FinalPressure = currentPressure + pressureChange;

        // Determine if encounter will end
        DetermineEncounterOutcome(projection, currentTurn);

        return projection;
    }

    private void CalculateSkillBonuses(CardDefinition choice, ChoiceProjection projection, ref int momentumChange, ref int pressureChange)
    {
    }

    private void CalculateBaseCardEffect(
        CardDefinition choice,
        ChoiceProjection projection,
        ref int momentumChange,
        ref int pressureChange)
    {
        projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
        {
            Source = $"Card Base Effect (Tier {choice.Level})",
            Value = choice.Gain
        });
    }

    private void CalculateEnvironmentalPressure(
        int currentTurn,
        ChoiceProjection projection,
        ref int pressureChange)
    {
        int environmentalPressure = _encounterInfo.GetEnvironmentalPressure(currentTurn);
        if (environmentalPressure > 0)
        {
            pressureChange += environmentalPressure;
            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Environmental Pressure",
                Value = environmentalPressure
            });
        }
    }

    private void EnsureNoNegativeValues(
        int currentMomentum,
        int currentPressure,
        ChoiceProjection projection,
        ref int momentumChange,
        ref int pressureChange)
    {
        if (currentMomentum + momentumChange < 0)
        {
            int adjustment = -(currentMomentum + momentumChange);
            momentumChange += adjustment;

            projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Minimum Momentum Limit",
                Value = adjustment
            });
        }

        if (currentPressure + pressureChange < 0)
        {
            int adjustment = -(currentPressure + pressureChange);
            pressureChange += adjustment;

            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Minimum Pressure Limit",
                Value = adjustment
            });
        }
    }

    private void DetermineEncounterOutcome(ChoiceProjection projection, int currentTurn)
    {
        int projectedTurn = currentTurn + 1;
        projection.ProjectedTurn = projectedTurn;

        bool encounterEnds =
            projectedTurn >= _encounterInfo.MaxTurns ||
            projection.FinalMomentum >= _encounterInfo.ExceptionalThreshold ||
            projection.FinalPressure >= _encounterInfo.MaxPressure;

        projection.EncounterWillEnd = encounterEnds;

        if (encounterEnds)
        {
            if (projection.FinalPressure >= _encounterInfo.MaxPressure)
            {
                projection.ProjectedOutcome = EncounterOutcomes.Failure;
            }
            else if (projection.FinalMomentum >= _encounterInfo.ExceptionalThreshold)
            {
                projection.ProjectedOutcome = EncounterOutcomes.Exceptional;
            }
            else if (projection.FinalMomentum >= _encounterInfo.StandardThreshold)
            {
                projection.ProjectedOutcome = EncounterOutcomes.Standard;
            }
            else if (projection.FinalMomentum >= _encounterInfo.PartialThreshold)
            {
                projection.ProjectedOutcome = EncounterOutcomes.Partial;
            }
            else
            {
                projection.ProjectedOutcome = EncounterOutcomes.Failure;
            }
        }
    }
}