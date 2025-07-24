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