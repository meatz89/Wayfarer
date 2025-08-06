using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Factory for creating standing obligations with guaranteed valid references.
/// Standing obligations reference NPCs as their source.
/// </summary>
public class StandingObligationFactory
{
    public StandingObligationFactory()
    {
        // No dependencies - factory is stateless
    }

    /// <summary>
    /// Create a minimal standing obligation with just an ID.
    /// Used for dummy/placeholder creation when references are missing.
    /// </summary>
    public StandingObligation CreateMinimalObligation(string id, string npcId = null)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Obligation ID cannot be empty", nameof(id));

        string name = FormatIdAsName(id);

        return new StandingObligation
        {
            ID = id,
            Name = name,
            Description = $"An obligation called {name}",
            Source = npcId ?? "unknown_npc",
            RelatedTokenType = ConnectionType.Trust, // Most basic type
            BenefitEffects = new List<ObligationEffect>(),
            ConstraintEffects = new List<ObligationEffect>()
        };
    }

    private string FormatIdAsName(string id)
    {
        // Convert snake_case or kebab-case to Title Case
        return string.Join(" ",
            id.Replace('_', ' ').Replace('-', ' ')
              .Split(' ')
              .Select(word => string.IsNullOrEmpty(word) ? "" :
                  char.ToUpper(word[0]) + word.Substring(1).ToLower()));
    }

    /// <summary>
    /// Create a standing obligation with validated references
    /// </summary>
    public StandingObligation CreateStandingObligation(
        string id,
        string name,
        string description,
        NPC source,
        ConnectionType? relatedTokenType,
        IEnumerable<ObligationEffect> benefitEffects = null,
        IEnumerable<ObligationEffect> constraintEffects = null)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Standing obligation ID cannot be empty", nameof(id));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Standing obligation name cannot be empty", nameof(name));
        if (string.IsNullOrEmpty(description))
            throw new ArgumentException("Standing obligation description cannot be empty", nameof(description));
        if (source == null)
            throw new ArgumentNullException(nameof(source), "Source NPC cannot be null");

        StandingObligation obligation = new StandingObligation
        {
            ID = id,
            Name = name,
            Description = description,
            Source = source.ID,
            RelatedTokenType = relatedTokenType,
            BenefitEffects = benefitEffects?.ToList() ?? new List<ObligationEffect>(),
            ConstraintEffects = constraintEffects?.ToList() ?? new List<ObligationEffect>()
        };

        return obligation;
    }

    /// <summary>
    /// Create a standing obligation from string IDs with validation
    /// </summary>
    public StandingObligation CreateStandingObligationFromIds(
        string id,
        string name,
        string description,
        string sourceNpcId,
        IEnumerable<NPC> availableNPCs,
        ConnectionType? relatedTokenType,
        IEnumerable<ObligationEffect> benefitEffects = null,
        IEnumerable<ObligationEffect> constraintEffects = null)
    {
        // Validate source NPC
        NPC? sourceNpc = availableNPCs.FirstOrDefault(n => n.ID == sourceNpcId);
        if (sourceNpc == null)
        {
            Console.WriteLine($"WARNING: Standing obligation '{id}' references non-existent source NPC '{sourceNpcId}'");
            // Create with source ID anyway for data preservation
            return new StandingObligation
            {
                ID = id,
                Name = name,
                Description = description,
                Source = sourceNpcId, // Keep the ID even if NPC doesn't exist
                RelatedTokenType = relatedTokenType,
                BenefitEffects = benefitEffects?.ToList() ?? new List<ObligationEffect>(),
                ConstraintEffects = constraintEffects?.ToList() ?? new List<ObligationEffect>()
            };
        }

        return CreateStandingObligation(id, name, description, sourceNpc,
                                       relatedTokenType, benefitEffects, constraintEffects);
    }

    /// <summary>
    /// Create a standing obligation from DTO with validation
    /// </summary>
    public StandingObligation CreateStandingObligationFromDTO(
        StandingObligationDTO dto,
        IEnumerable<NPC> availableNPCs)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        // Parse token type
        ConnectionType? tokenType = null;
        if (!string.IsNullOrEmpty(dto.RelatedTokenType))
        {
            if (EnumParser.TryParse<ConnectionType>(dto.RelatedTokenType, out ConnectionType parsed))
            {
                tokenType = parsed;
            }
            else
            {
                Console.WriteLine($"WARNING: Unknown token type '{dto.RelatedTokenType}' for obligation '{dto.ID}'");
            }
        }

        // Parse benefit effects
        List<ObligationEffect> benefitEffects = new List<ObligationEffect>();
        foreach (string effectStr in dto.BenefitEffects ?? new List<string>())
        {
            if (EnumParser.TryParse<ObligationEffect>(effectStr, out ObligationEffect effect))
            {
                benefitEffects.Add(effect);
            }
            else
            {
                Console.WriteLine($"WARNING: Unknown benefit effect '{effectStr}' for obligation '{dto.ID}'");
            }
        }

        // Parse constraint effects
        List<ObligationEffect> constraintEffects = new List<ObligationEffect>();
        foreach (string effectStr in dto.ConstraintEffects ?? new List<string>())
        {
            if (EnumParser.TryParse<ObligationEffect>(effectStr, out ObligationEffect effect))
            {
                constraintEffects.Add(effect);
            }
            else
            {
                Console.WriteLine($"WARNING: Unknown constraint effect '{effectStr}' for obligation '{dto.ID}'");
            }
        }

        StandingObligation obligation = CreateStandingObligationFromIds(
            dto.ID,
            dto.Name,
            dto.Description,
            dto.Source,
            availableNPCs,
            tokenType,
            benefitEffects,
            constraintEffects);

        // Apply threshold-based activation settings
        obligation.RelatedNPCId = dto.RelatedNPCId;
        obligation.ActivationThreshold = dto.ActivationThreshold;
        obligation.DeactivationThreshold = dto.DeactivationThreshold;
        obligation.IsThresholdBased = dto.IsThresholdBased;
        obligation.ActivatesAboveThreshold = dto.ActivatesAboveThreshold;

        // Apply dynamic scaling settings
        if (!string.IsNullOrEmpty(dto.ScalingType) &&
            EnumParser.TryParse<ScalingType>(dto.ScalingType, out ScalingType scalingType))
        {
            obligation.ScalingType = scalingType;
        }

        obligation.ScalingFactor = dto.ScalingFactor;
        obligation.BaseValue = dto.BaseValue;
        obligation.MinValue = dto.MinValue;
        obligation.MaxValue = dto.MaxValue;

        if (dto.SteppedThresholds != null && dto.SteppedThresholds.Any())
        {
            obligation.SteppedThresholds = new Dictionary<int, float>(dto.SteppedThresholds);
        }

        return obligation;
    }
}