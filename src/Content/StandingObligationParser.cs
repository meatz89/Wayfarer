using System;
using System.Text.Json;
using Wayfarer.GameState.Enums;

/// <summary>
/// Parser for deserializing standing obligation data from JSON.
/// </summary>
public static class StandingObligationParser
{
    /// <summary>
    /// Convert a StandingObligationDTO to a StandingObligation domain model
    /// </summary>
    public static StandingObligation ConvertDTOToStandingObligation(StandingObligationDTO dto)
    {
        StandingObligation obligation = new StandingObligation
        {
            ID = dto.ID ?? "",
            Name = dto.Name ?? "",
            Description = dto.Description ?? "",
            Source = dto.Source ?? "",
            RelatedNPCId = dto.RelatedNPCId,
            ActivationThreshold = dto.ActivationThreshold,
            DeactivationThreshold = dto.DeactivationThreshold,
            IsThresholdBased = dto.IsThresholdBased,
            ActivatesAboveThreshold = dto.ActivatesAboveThreshold,
            ScalingFactor = dto.ScalingFactor,
            BaseValue = dto.BaseValue,
            MinValue = dto.MinValue,
            MaxValue = dto.MaxValue,
            SteppedThresholds = dto.SteppedThresholds ?? new Dictionary<int, float>()
        };

        // Parse related token type
        if (!string.IsNullOrEmpty(dto.RelatedTokenType))
        {
            if (EnumParser.TryParse<ConnectionType>(dto.RelatedTokenType, out ConnectionType tokenType))
            {
                obligation.RelatedTokenType = tokenType;
            }
        }

        // Parse scaling type
        if (!string.IsNullOrEmpty(dto.ScalingType))
        {
            if (EnumParser.TryParse<ScalingType>(dto.ScalingType, out ScalingType scalingType))
            {
                obligation.ScalingType = scalingType;
            }
        }

        // Parse benefit effects
        if (dto.BenefitEffects != null)
        {
            foreach (string effect in dto.BenefitEffects)
            {
                if (EnumParser.TryParse<ObligationEffect>(effect, out ObligationEffect obligationEffect))
                {
                    obligation.BenefitEffects.Add(obligationEffect);
                }
            }
        }

        // Parse constraint effects
        if (dto.ConstraintEffects != null)
        {
            foreach (string effect in dto.ConstraintEffects)
            {
                if (EnumParser.TryParse<ObligationEffect>(effect, out ObligationEffect obligationEffect))
                {
                    obligation.ConstraintEffects.Add(obligationEffect);
                }
            }
        }

        return obligation;
    }
    public static StandingObligation ParseStandingObligation(string json)
    {
        JsonDocumentOptions options = new JsonDocumentOptions
        {
            AllowTrailingCommas = true
        };

        using JsonDocument doc = JsonDocument.Parse(json, options);
        JsonElement root = doc.RootElement;

        return ParseStandingObligation(root);
    }

    public static StandingObligation ParseStandingObligation(JsonElement element)
    {
        StandingObligation obligation = new StandingObligation();

        if (element.TryGetProperty("ID", out JsonElement idElement))
            obligation.ID = idElement.GetString() ?? "";

        if (element.TryGetProperty("Name", out JsonElement nameElement))
            obligation.Name = nameElement.GetString() ?? "";

        if (element.TryGetProperty("Description", out JsonElement descElement))
            obligation.Description = descElement.GetString() ?? "";

        if (element.TryGetProperty("Source", out JsonElement sourceElement))
            obligation.Source = sourceElement.GetString() ?? "";

        if (element.TryGetProperty("RelatedTokenType", out JsonElement tokenElement) &&
            !tokenElement.ValueKind.Equals(JsonValueKind.Null))
        {
            if (EnumParser.TryParse<ConnectionType>(tokenElement.GetString(), out ConnectionType tokenType))
                obligation.RelatedTokenType = tokenType;
        }

        if (element.TryGetProperty("BenefitEffects", out JsonElement benefitsElement))
        {
            foreach (JsonElement benefitElement in benefitsElement.EnumerateArray())
            {
                if (EnumParser.TryParse<ObligationEffect>(benefitElement.GetString(), out ObligationEffect effect))
                    obligation.BenefitEffects.Add(effect);
            }
        }

        if (element.TryGetProperty("ConstraintEffects", out JsonElement constraintsElement))
        {
            foreach (JsonElement constraintElement in constraintsElement.EnumerateArray())
            {
                if (EnumParser.TryParse<ObligationEffect>(constraintElement.GetString(), out ObligationEffect effect))
                    obligation.ConstraintEffects.Add(effect);
            }
        }

        return obligation;
    }
}