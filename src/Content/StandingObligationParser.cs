using System;
using System.IO;
using System.Text.Json;

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
        // Validate required fields
        if (string.IsNullOrEmpty(dto.ID))
            throw new InvalidDataException("StandingObligation missing required field 'ID'");
        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidDataException($"StandingObligation '{dto.ID}' missing required field 'Name'");
        if (string.IsNullOrEmpty(dto.Description))
            throw new InvalidDataException($"StandingObligation '{dto.ID}' missing required field 'Description'");
        if (string.IsNullOrEmpty(dto.Source))
            throw new InvalidDataException($"StandingObligation '{dto.ID}' missing required field 'Source'");

        StandingObligation obligation = new StandingObligation
        {
            ID = dto.ID,
            Name = dto.Name,
            Description = dto.Description,
            Source = dto.Source,
            RelatedNPCId = dto.RelatedNPCId,
            ActivationThreshold = dto.ActivationThreshold,
            DeactivationThreshold = dto.DeactivationThreshold,
            IsThresholdBased = dto.IsThresholdBased,
            ActivatesAboveThreshold = dto.ActivatesAboveThreshold,
            ScalingFactor = dto.ScalingFactor,
            BaseValue = dto.BaseValue,
            MinValue = dto.MinValue,
            MaxValue = dto.MaxValue,
            SteppedThresholds = dto.SteppedThresholds // DTO has inline init, trust it
        };

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

        // Validate required string fields
        if (element.TryGetProperty("ID", out JsonElement idElement))
        {
            string id = idElement.GetString();
            if (string.IsNullOrEmpty(id))
                throw new InvalidDataException("StandingObligation missing required field 'ID'");
            obligation.ID = id;
        }
        else
        {
            throw new InvalidDataException("StandingObligation missing required field 'ID'");
        }

        if (element.TryGetProperty("Name", out JsonElement nameElement))
        {
            string name = nameElement.GetString();
            if (string.IsNullOrEmpty(name))
                throw new InvalidDataException($"StandingObligation '{obligation.ID}' missing required field 'Name'");
            obligation.Name = name;
        }
        else
        {
            throw new InvalidDataException($"StandingObligation '{obligation.ID}' missing required field 'Name'");
        }

        if (element.TryGetProperty("Description", out JsonElement descElement))
        {
            string description = descElement.GetString();
            if (string.IsNullOrEmpty(description))
                throw new InvalidDataException($"StandingObligation '{obligation.ID}' missing required field 'Description'");
            obligation.Description = description;
        }
        else
        {
            throw new InvalidDataException($"StandingObligation '{obligation.ID}' missing required field 'Description'");
        }

        if (element.TryGetProperty("Source", out JsonElement sourceElement))
        {
            string source = sourceElement.GetString();
            if (string.IsNullOrEmpty(source))
                throw new InvalidDataException($"StandingObligation '{obligation.ID}' missing required field 'Source'");
            obligation.Source = source;
        }
        else
        {
            throw new InvalidDataException($"StandingObligation '{obligation.ID}' missing required field 'Source'");
        }

        // Optional enum field - defaults to None if missing/invalid
        if (element.TryGetProperty("RelatedTokenType", out JsonElement tokenElement) &&
            !tokenElement.ValueKind.Equals(JsonValueKind.Null))
        {
            if (EnumParser.TryParse<ConnectionType>(tokenElement.GetString(), out ConnectionType tokenType))
                obligation.RelatedTokenType = tokenType;
        }

        // Parse benefit effects array if present
        if (element.TryGetProperty("BenefitEffects", out JsonElement benefitsElement))
        {
            foreach (JsonElement benefitElement in benefitsElement.EnumerateArray())
            {
                if (EnumParser.TryParse<ObligationEffect>(benefitElement.GetString(), out ObligationEffect effect))
                    obligation.BenefitEffects.Add(effect);
            }
        }

        // Parse constraint effects array if present
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