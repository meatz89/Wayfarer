/// <summary>
/// Parser for converting CompoundRequirement and NumericRequirement DTOs to domain models
/// Handles OR-based unlocking system for situations
/// </summary>
public static class RequirementParser
{
    /// <summary>
    /// Convert a CompoundRequirementDTO to a CompoundRequirement domain model
    /// </summary>
    public static CompoundRequirement ConvertDTOToCompoundRequirement(CompoundRequirementDTO dto)
    {
        if (dto == null)
            return new CompoundRequirement(); // Empty requirement = always unlocked

        CompoundRequirement requirement = new CompoundRequirement
        {
            OrPaths = ParseOrPaths(dto.OrPaths)
        };

        return requirement;
    }

    /// <summary>
    /// Parse a list of OR paths from DTOs
    /// Each path represents one complete way to unlock the situation
    /// </summary>
    private static List<OrPath> ParseOrPaths(List<OrPathDTO> dtos)
    {
        if (dtos == null || !dtos.Any())
            return new List<OrPath>();

        List<OrPath> orPaths = new List<OrPath>();
        foreach (OrPathDTO dto in dtos)
        {
            try
            {
                OrPath path = ConvertDTOToOrPath(dto);
                orPaths.Add(path);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse OR path '{dto?.Label}': {ex.Message}", ex);
            }
        }

        return orPaths;
    }

    /// <summary>
    /// Convert an OrPathDTO to an OrPath domain model
    /// </summary>
    private static OrPath ConvertDTOToOrPath(OrPathDTO dto)
    {
        if (dto == null)
            throw new InvalidOperationException("OrPath DTO cannot be null");

        OrPath path = new OrPath
        {
            Label = dto.Label ?? "Unlabeled Path",
            NumericRequirements = ParseNumericRequirements(dto.NumericRequirements, dto.Label)
        };

        return path;
    }

    /// <summary>
    /// Parse a list of numeric requirements from DTOs
    /// All requirements in a single path must be met (AND logic)
    /// </summary>
    private static List<NumericRequirement> ParseNumericRequirements(List<NumericRequirementDTO> dtos, string pathLabel)
    {
        if (dtos == null || !dtos.Any())
            return new List<NumericRequirement>();

        List<NumericRequirement> requirements = new List<NumericRequirement>();
        foreach (NumericRequirementDTO dto in dtos)
        {
            try
            {
                NumericRequirement requirement = ConvertDTOToNumericRequirement(dto);
                requirements.Add(requirement);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse numeric requirement in path '{pathLabel}': {ex.Message}", ex);
            }
        }

        return requirements;
    }

    /// <summary>
    /// Convert a NumericRequirementDTO to a NumericRequirement domain model
    /// </summary>
    private static NumericRequirement ConvertDTOToNumericRequirement(NumericRequirementDTO dto)
    {
        if (dto == null)
            throw new InvalidOperationException("NumericRequirement DTO cannot be null");

        if (string.IsNullOrEmpty(dto.Type))
            throw new InvalidOperationException("NumericRequirement missing required 'Type' field");

        // Validate Type is a known value
        List<string> validTypes = new List<string>
        {
            "BondStrength", "Scale", "Resolve", "Coins",
            "CompletedSituations", "Achievement", "State", "PlayerStat"
        };

        if (!validTypes.Contains(dto.Type))
        {
            throw new InvalidOperationException($"NumericRequirement has invalid Type value: '{dto.Type}'. Must be one of: {string.Join(", ", validTypes)}");
        }

        // Validate Context is present when required
        if (RequiresContext(dto.Type) && string.IsNullOrEmpty(dto.Context))
        {
            throw new InvalidOperationException($"NumericRequirement of type '{dto.Type}' requires 'Context' field (NPC ID, Scale name, Achievement ID, or State type)");
        }

        NumericRequirement requirement = new NumericRequirement
        {
            Type = dto.Type,
            Context = dto.Context,
            Threshold = dto.Threshold,
            Label = dto.Label ?? GenerateDefaultLabel(dto)
        };

        return requirement;
    }

    /// <summary>
    /// Check if a requirement type requires a Context field
    /// </summary>
    private static bool RequiresContext(string type)
    {
        return type switch
        {
            "BondStrength" => true,  // Needs NPC ID
            "Scale" => true,         // Needs scale name
            "Achievement" => true,   // Needs achievement ID
            "State" => true,         // Needs state type
            "PlayerStat" => true,    // Needs stat name (Insight, Rapport, Authority, Diplomacy, Cunning)
            "Resolve" => false,      // No context needed
            "Coins" => false,        // No context needed
            "CompletedSituations" => false, // No context needed
            _ => false
        };
    }

    /// <summary>
    /// Generate a default label if none provided in JSON
    /// </summary>
    private static string GenerateDefaultLabel(NumericRequirementDTO dto)
    {
        return dto.Type switch
        {
            "BondStrength" => $"Bond {dto.Threshold}+ with {dto.Context}",
            "Scale" => $"{dto.Context} {dto.Threshold:+#;-#;0}",
            "Resolve" => $"Resolve {dto.Threshold}+",
            "Coins" => $"{dto.Threshold} coins",
            "CompletedSituations" => $"{dto.Threshold} situations completed",
            "Achievement" => dto.Threshold > 0 ? $"Have {dto.Context}" : $"Don't have {dto.Context}",
            "State" => dto.Threshold > 0 ? $"Have {dto.Context} state" : $"Don't have {dto.Context} state",
            "PlayerStat" => $"{dto.Context} {dto.Threshold}+",
            _ => "Unknown requirement"
        };
    }
}
