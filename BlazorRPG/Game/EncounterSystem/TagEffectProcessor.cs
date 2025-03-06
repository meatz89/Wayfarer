/// <summary>
/// Centralized processor for all tag effects
/// </summary>
public class TagEffectProcessor
{
    /// <summary>
    /// Process all effects from a collection of tags on a choice projection
    /// </summary>
    public void ProcessTagEffects(
        List<EncounterTag> activeTags,
        Choice choice,
        ChoiceOutcome baseOutcome,
        ChoiceProjection projection)
    {
        ChoiceOutcome currentOutcome = new ChoiceOutcome(baseOutcome.Momentum, baseOutcome.Pressure);

        // First pass: Process standard effects
        foreach (EncounterTag tag in activeTags)
        {
            // Skip if tag doesn't apply to this choice
            if (!DoeTagApplyToChoice(tag, choice))
                continue;

            ChoiceOutcome before = new ChoiceOutcome(currentOutcome.Momentum, currentOutcome.Pressure);

            // Apply appropriate effect based on type
            switch (tag.Effect.EffectType)
            {
                case TagEffectType.Standard:
                    ApplyStandardEffect(tag.Effect, ref currentOutcome);
                    break;

                case TagEffectType.ConvertPressureToMomentum:
                    // Will be applied in second pass
                    break;

                case TagEffectType.ReducePressureEachTurn:
                    // Will be applied in second pass
                    break;

                case TagEffectType.AdditionalTurn:
                    // Will be applied in second pass
                    break;

                case TagEffectType.AutoSuccess:
                    // Will be applied in second pass
                    break;

                case TagEffectType.PressureImmunity:
                    ApplyPressureImmunity(tag.Effect, ref currentOutcome);
                    break;
            }

            // Record effect for this tag
            RecordTagEffect(tag, before, currentOutcome, projection);
        }

        // Keep track of pre-special effect values for second pass
        int preMomentum = currentOutcome.Momentum;
        int prePressure = currentOutcome.Pressure;

        // Project end-of-turn pressure (normally +1)
        currentOutcome.Pressure += 1;
        projection.BasePressureChange += 1;

        // Calculate intermediate values before special effects
        projection.MomentumChange = currentOutcome.Momentum;
        projection.PressureChange = currentOutcome.Pressure;
        projection.ProjectedMomentum = projection.CurrentMomentum + projection.MomentumChange;
        projection.ProjectedPressure = projection.CurrentPressure + projection.PressureChange;

        // Second pass: Process special effects that need to know the final values
        foreach (EncounterTag tag in activeTags)
        {
            // Special effects need to know the projected values, not just the outcome
            ChoiceOutcome before = new ChoiceOutcome(currentOutcome.Momentum, currentOutcome.Pressure);
            int projectedMomentumBefore = projection.ProjectedMomentum;
            int projectedPressureBefore = projection.ProjectedPressure;

            // Apply appropriate special effect
            switch (tag.Effect.EffectType)
            {
                case TagEffectType.ConvertPressureToMomentum:
                    ApplyConvertPressureEffect(tag.Effect, projection);
                    break;

                case TagEffectType.ReducePressureEachTurn:
                    ApplyReducePressureEffect(tag.Effect, projection);
                    break;

                case TagEffectType.AdditionalTurn:
                    ApplyAdditionalTurnEffect(tag.Effect, projection);
                    break;

                case TagEffectType.AutoSuccess:
                    ApplyAutoSuccessEffect(tag.Effect, projection);
                    break;
            }

            // Record special effect if it had an impact
            if (projection.ProjectedMomentum != projectedMomentumBefore ||
                projection.ProjectedPressure != projectedPressureBefore)
            {
                // Update current outcome to reflect special effect
                currentOutcome.Momentum = projection.ProjectedMomentum - projection.CurrentMomentum;
                currentOutcome.Pressure = projection.ProjectedPressure - projection.CurrentPressure;

                // Record the effect
                RecordSpecialEffect(tag, projectedMomentumBefore, projectedPressureBefore, projection);
            }
        }

        // Ensure projection changes reflect final outcome
        projection.MomentumChange = projection.ProjectedMomentum - projection.CurrentMomentum;
        projection.PressureChange = projection.ProjectedPressure - projection.CurrentPressure;
    }

    /// <summary>
    /// Check if a tag applies to the given choice
    /// </summary>
    private bool DoeTagApplyToChoice(EncounterTag tag, Choice choice)
    {
        // Check if tag applies based on focus and approach
        if (tag.Effect.AffectedFocus.HasValue && choice.FocusType != tag.Effect.AffectedFocus.Value)
            return false;

        if (tag.Effect.AffectedApproach.HasValue && choice.ApproachType != tag.Effect.AffectedApproach.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Apply standard momentum and pressure modifications
    /// </summary>
    private void ApplyStandardEffect(TagEffect effect, ref ChoiceOutcome outcome)
    {
        // Apply momentum modifier
        outcome.Momentum += effect.MomentumModifier;

        // Apply pressure modifier
        outcome.Pressure += effect.PressureModifier;

        // Apply special flags
        if (effect.ZeroPressure)
        {
            outcome.Pressure = 0;
        }

        if (effect.DoubleMomentum)
        {
            outcome.Momentum *= 2;
        }

        // Apply negative effects
        if (effect.IsNegative)
        {
            if (effect.BlockMomentum)
            {
                outcome.Momentum = 0;
            }

            if (effect.DoublePressure)
            {
                outcome.Pressure *= 2;
            }
        }
    }

    /// <summary>
    /// Apply pressure immunity effect
    /// </summary>
    private void ApplyPressureImmunity(TagEffect effect, ref ChoiceOutcome outcome)
    {
        // Zero out pressure gain
        outcome.Pressure = 0;
    }

    /// <summary>
    /// Apply pressure-to-momentum conversion effect
    /// </summary>
    private void ApplyConvertPressureEffect(TagEffect effect, ChoiceProjection projection)
    {
        // Convert the pressure change into momentum
        int pressureChange = projection.PressureChange;

        if (pressureChange > 0)
        {
            // Update projection values
            projection.ProjectedMomentum += pressureChange;
            projection.ProjectedPressure -= pressureChange;
        }
    }

    /// <summary>
    /// Apply pressure reduction at end of turn
    /// </summary>
    private void ApplyReducePressureEffect(TagEffect effect, ChoiceProjection projection)
    {
        int reductionAmount = effect.PressureModifier;

        if (projection.ProjectedPressure > 0)
        {
            // Ensure we don't go below 0
            projection.ProjectedPressure = Math.Max(0, projection.ProjectedPressure + reductionAmount);
        }
    }

    /// <summary>
    /// Apply additional turn effect
    /// </summary>
    private void ApplyAdditionalTurnEffect(TagEffect effect, ChoiceProjection projection)
    {
        // For now, just zero out pressure
        projection.ProjectedPressure = 0;
    }

    /// <summary>
    /// Apply auto-success effect
    /// </summary>
    private void ApplyAutoSuccessEffect(TagEffect effect, ChoiceProjection projection)
    {
        int requiredMomentum = 15; // Or whatever defines success

        if (projection.ProjectedMomentum < requiredMomentum)
        {
            projection.ProjectedMomentum = requiredMomentum;
        }
    }

    /// <summary>
    /// Record the effect of a tag in the projection
    /// </summary>
    private void RecordTagEffect(
        EncounterTag tag,
        ChoiceOutcome before,
        ChoiceOutcome after,
        ChoiceProjection projection)
    {
        int momentumEffect = after.Momentum - before.Momentum;
        int pressureEffect = after.Pressure - before.Pressure;

        if (momentumEffect != 0)
        {
            projection.TagMomentumEffects[tag.Name] = momentumEffect;
        }

        if (pressureEffect != 0)
        {
            projection.TagPressureEffects[tag.Name] = pressureEffect;
        }
    }

    /// <summary>
    /// Record a special effect's impact
    /// </summary>
    private void RecordSpecialEffect(
        EncounterTag tag,
        int momentumBefore,
        int pressureBefore,
        ChoiceProjection projection)
    {
        int momentumEffect = projection.ProjectedMomentum - momentumBefore;
        int pressureEffect = projection.ProjectedPressure - pressureBefore;

        if (momentumEffect != 0)
        {
            projection.TagMomentumEffects[tag.Name] = momentumEffect;
        }

        if (pressureEffect != 0)
        {
            projection.TagPressureEffects[tag.Name] = pressureEffect;
        }

        projection.SpecialEffects.Add(tag.Name);
    }
}