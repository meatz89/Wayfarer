using System;
using System.Text.Json;

/// <summary>
/// Parser for deserializing standing obligation data from JSON.
/// </summary>
public static class StandingObligationParser
{
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
        var obligation = new StandingObligation();

        if (element.TryGetProperty("ID", out var idElement))
            obligation.ID = idElement.GetString() ?? "";

        if (element.TryGetProperty("Name", out var nameElement))
            obligation.Name = nameElement.GetString() ?? "";

        if (element.TryGetProperty("Description", out var descElement))
            obligation.Description = descElement.GetString() ?? "";

        if (element.TryGetProperty("Source", out var sourceElement))
            obligation.Source = sourceElement.GetString() ?? "";

        if (element.TryGetProperty("RelatedTokenType", out var tokenElement) && 
            !tokenElement.ValueKind.Equals(JsonValueKind.Null))
        {
            if (Enum.TryParse<ConnectionType>(tokenElement.GetString(), out var tokenType))
                obligation.RelatedTokenType = tokenType;
        }

        if (element.TryGetProperty("BenefitEffects", out var benefitsElement))
        {
            foreach (var benefitElement in benefitsElement.EnumerateArray())
            {
                if (Enum.TryParse<ObligationEffect>(benefitElement.GetString(), out var effect))
                    obligation.BenefitEffects.Add(effect);
            }
        }

        if (element.TryGetProperty("ConstraintEffects", out var constraintsElement))
        {
            foreach (var constraintElement in constraintsElement.EnumerateArray())
            {
                if (Enum.TryParse<ObligationEffect>(constraintElement.GetString(), out var effect))
                    obligation.ConstraintEffects.Add(effect);
            }
        }

        return obligation;
    }
}