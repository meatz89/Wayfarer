using System.Text.Json;

/// <summary>
/// Parser for deserializing standing obligation data from JSON.
/// </summary>
public static class StandingObligationParser
{
    /// <summary>
    /// Convert a StandingObligationDTO to a StandingObligation domain model
    /// HIGHLANDER: Resolves NPC object reference from RelatedNPCId during parsing
    /// </summary>
    public static StandingObligation ConvertDTOToStandingObligation(StandingObligationDTO dto, GameWorld gameWorld)
    {
        // Validate required fields
        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidDataException($"StandingObligation missing required field 'Name'");
        if (string.IsNullOrEmpty(dto.Description))
            throw new InvalidDataException($"StandingObligation '{dto.Name}' missing required field 'Description'");
        if (string.IsNullOrEmpty(dto.Source))
            throw new InvalidDataException($"StandingObligation '{dto.Name}' missing required field 'Source'");

        StandingObligation obligation = new StandingObligation
        {
            Name = dto.Name,
            Description = dto.Description,
            Source = dto.Source,
            ActivationThreshold = dto.ActivationThreshold,
            DeactivationThreshold = dto.DeactivationThreshold,
            IsThresholdBased = dto.IsThresholdBased,
            ActivatesAboveThreshold = dto.ActivatesAboveThreshold,
            ScalingFactor = dto.ScalingFactor,
            BaseValue = dto.BaseValue,
            MinValue = dto.MinValue,
            MaxValue = dto.MaxValue,
            SteppedThresholds = dto.SteppedThresholds
        };

        // Resolve NPC object reference from RelatedNPCId (parse-time translation)
        if (!string.IsNullOrEmpty(dto.RelatedNPCId))
        {
            NPC relatedNpc = gameWorld.NPCs.FirstOrDefault(n => n.Name == dto.RelatedNPCId);
            if (relatedNpc == null)
                throw new InvalidDataException($"StandingObligation '{dto.Name}' references unknown NPC '{dto.RelatedNPCId}'");
            obligation.RelatedNPC = relatedNpc;
        }

        // Parse related token type - optional field, defaults to None if missing/invalid
        if (!string.IsNullOrEmpty(dto.RelatedTokenType))
        {
            if (EnumParser.TryParse<ConnectionType>(dto.RelatedTokenType, out ConnectionType tokenType))
            {
                obligation.RelatedTokenType = tokenType;
            }
        }

        // Parse scaling type - optional field, defaults to None if missing/invalid
        if (!string.IsNullOrEmpty(dto.ScalingType))
        {
            if (EnumParser.TryParse<ScalingType>(dto.ScalingType, out ScalingType scalingType))
            {
                obligation.ScalingType = scalingType;
            }
        }

        // Parse benefit effects - DTO has inline init, trust it
        foreach (string effect in dto.BenefitEffects)
        {
            if (EnumParser.TryParse<ObligationEffect>(effect, out ObligationEffect obligationEffect))
            {
                obligation.BenefitEffects.Add(obligationEffect);
            }
        }

        // Parse constraint effects - DTO has inline init, trust it
        foreach (string effect in dto.ConstraintEffects)
        {
            if (EnumParser.TryParse<ObligationEffect>(effect, out ObligationEffect obligationEffect))
            {
                obligation.ConstraintEffects.Add(obligationEffect);
            }
        }

        return obligation;
    }
}