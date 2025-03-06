/// <summary>
/// Represents the effect a tag has on gameplay
/// </summary>
public class TagEffect
{
    // Basic effects
    public FocusTypes? AffectedFocus { get; set; }
    public ApproachTypes? AffectedApproach { get; set; }
    public int MomentumModifier { get; set; }
    public int PressureModifier { get; set; }

    // Boolean flags
    public bool IsNegative { get; set; }
    public bool ZeroPressure { get; set; }
    public bool DoubleMomentum { get; set; }
    public bool BlockMomentum { get; set; }
    public bool DoublePressure { get; set; }

    // Effect type enumeration - replaces IsSpecialEffect and SpecialEffectId
    public TagEffectType EffectType { get; set; } = TagEffectType.Standard;

    /// <summary>
    /// Apply this effect to a choice projection
    /// </summary>
    public void ApplyToProjection(Choice choice, ChoiceProjection projection, string tagName)
    {
        // Record changes for explanation UI
        int momentumBefore = projection.MomentumChange;
        int pressureBefore = projection.PressureChange;

        // Check if this tag applies to the choice
        bool tagApplies = IsApplicableToChoice(choice);

        if (tagApplies)
        {
            // Apply the appropriate effect based on type
            switch (EffectType)
            {
                case TagEffectType.Standard:
                    ApplyStandardEffect(projection);
                    break;

                case TagEffectType.ConvertPressureToMomentum:
                    ApplyPressureConversion(projection);
                    break;

                case TagEffectType.ReducePressureEachTurn:
                    ApplyPressureReduction(projection);
                    break;

                case TagEffectType.AdditionalTurnNoPressure:
                    ApplyAdditionalTurn(projection);
                    break;

                case TagEffectType.EncounterAutoSuccess:
                    ApplyAutoSuccess(projection);
                    break;
            }
        }

        // Record tag effects for UI
        int momentumEffect = projection.MomentumChange - momentumBefore;
        int pressureEffect = projection.PressureChange - pressureBefore;

        if (momentumEffect != 0)
        {
            projection.TagMomentumEffects[tagName] = momentumEffect;
        }

        if (pressureEffect != 0)
        {
            projection.TagPressureEffects[tagName] = pressureEffect;
        }

        // Record special effect application
        if (EffectType != TagEffectType.Standard)
        {
            projection.SpecialEffects.Add($"{tagName} ({EffectType})");
        }
    }

    /// <summary>
    /// Check if this effect applies to the given choice
    /// </summary>
    private bool IsApplicableToChoice(Choice choice)
    {
        // If no specific focus or approach is specified, the tag applies to all choices
        if (!AffectedFocus.HasValue && !AffectedApproach.HasValue)
        {
            return true;
        }

        // Check if the choice matches the specified focus
        if (AffectedFocus.HasValue && choice.FocusType == AffectedFocus.Value)
        {
            return true;
        }

        // Check if the choice matches the specified approach
        if (AffectedApproach.HasValue && choice.ApproachType == AffectedApproach.Value)
        {
            return true;
        }

        // Neither focus nor approach matched
        return false;
    }

    /// <summary>
    /// Apply standard momentum and pressure modifications
    /// </summary>
    private void ApplyStandardEffect(ChoiceProjection projection)
    {
        // Apply basic modifiers
        projection.MomentumChange += MomentumModifier;
        projection.PressureChange += PressureModifier;
        projection.ProjectedMomentum += MomentumModifier;
        projection.ProjectedPressure += PressureModifier;

        // Apply special boolean flags
        if (ZeroPressure)
        {
            int currentPressure = projection.ProjectedPressure;
            projection.PressureChange -= currentPressure;
            projection.ProjectedPressure = 0;
        }

        if (DoubleMomentum)
        {
            int additionalMomentum = projection.MomentumChange;
            projection.MomentumChange += additionalMomentum;
            projection.ProjectedMomentum += additionalMomentum;
        }

        if (BlockMomentum && IsNegative)
        {
            int currentMomentum = projection.MomentumChange;
            projection.MomentumChange -= currentMomentum;
            projection.ProjectedMomentum -= currentMomentum;
        }

        if (DoublePressure && IsNegative)
        {
            int additionalPressure = projection.PressureChange;
            projection.PressureChange += additionalPressure;
            projection.ProjectedPressure += additionalPressure;
        }
    }

    /// <summary>
    /// Apply pressure conversion effect
    /// </summary>
    private void ApplyPressureConversion(ChoiceProjection projection)
    {
        // Calculate the pressure to convert
        int pressureToConvert = projection.PressureChange;

        // Add the pressure amount to momentum
        projection.MomentumChange += pressureToConvert;
        projection.ProjectedMomentum += pressureToConvert;

        // Remove the pressure
        projection.PressureChange -= pressureToConvert;
        projection.ProjectedPressure -= pressureToConvert;
    }

    /// <summary>
    /// Apply pressure reduction at end of turn
    /// </summary>
    private void ApplyPressureReduction(ChoiceProjection projection)
    {
        // Only apply if pressure would be greater than 0
        if (projection.ProjectedPressure > 0)
        {
            int reductionAmount = PressureModifier != 0 ? PressureModifier : -1;

            projection.PressureChange += reductionAmount;
            projection.ProjectedPressure += reductionAmount;
        }
    }

    /// <summary>
    /// Apply additional turn with no pressure effect
    /// </summary>
    private void ApplyAdditionalTurn(ChoiceProjection projection)
    {
        // Zero out pressure for this turn
        int currentPressure = projection.ProjectedPressure;
        projection.PressureChange -= currentPressure;
        projection.ProjectedPressure = 0;

        // Flag that an additional turn will be granted
        projection.GrantsAdditionalTurn = true;
    }

    /// <summary>
    /// Apply auto-success effect
    /// </summary>
    private void ApplyAutoSuccess(ChoiceProjection projection)
    {
        // Ensure momentum is at least 15 (or other success threshold)
        int requiredMomentum = 15;
        if (projection.ProjectedMomentum < requiredMomentum)
        {
            int momentumDifference = requiredMomentum - projection.ProjectedMomentum;
            projection.MomentumChange += momentumDifference;
            projection.ProjectedMomentum = requiredMomentum;
        }
    }
}

/// <summary>
/// Types of tag effects
/// </summary>
public enum TagEffectType
{
    Standard,
    ConvertPressureToMomentum,
    ReducePressureEachTurn,
    AdditionalTurnNoPressure,
    EncounterAutoSuccess,
    AdditionalTurn,
    AutoSuccess,
    PressureImmunity
}

