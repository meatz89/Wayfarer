using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.Content.Utilities;

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
        
        var obligation = new StandingObligation
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
        var sourceNpc = availableNPCs.FirstOrDefault(n => n.ID == sourceNpcId);
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
            if (EnumParser.TryParse<ConnectionType>(dto.RelatedTokenType, out var parsed))
            {
                tokenType = parsed;
            }
            else
            {
                Console.WriteLine($"WARNING: Unknown token type '{dto.RelatedTokenType}' for obligation '{dto.ID}'");
            }
        }
        
        // Parse benefit effects
        var benefitEffects = new List<ObligationEffect>();
        foreach (var effectStr in dto.BenefitEffects ?? new List<string>())
        {
            if (EnumParser.TryParse<ObligationEffect>(effectStr, out var effect))
            {
                benefitEffects.Add(effect);
            }
            else
            {
                Console.WriteLine($"WARNING: Unknown benefit effect '{effectStr}' for obligation '{dto.ID}'");
            }
        }
        
        // Parse constraint effects
        var constraintEffects = new List<ObligationEffect>();
        foreach (var effectStr in dto.ConstraintEffects ?? new List<string>())
        {
            if (EnumParser.TryParse<ObligationEffect>(effectStr, out var effect))
            {
                constraintEffects.Add(effect);
            }
            else
            {
                Console.WriteLine($"WARNING: Unknown constraint effect '{effectStr}' for obligation '{dto.ID}'");
            }
        }
        
        return CreateStandingObligationFromIds(
            dto.ID,
            dto.Name,
            dto.Description,
            dto.Source,
            availableNPCs,
            tokenType,
            benefitEffects,
            constraintEffects);
    }
}