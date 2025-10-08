using System.Collections.Generic;

/// <summary>
/// Parser for Knowledge definitions - converts DTOs to domain models
/// Stateless parser following ARCHITECTURE.md principles
/// </summary>
public static class KnowledgeParser
{
    public static Knowledge ParseKnowledge(KnowledgeDTO dto)
    {
        if (dto == null)
            throw new System.ArgumentNullException(nameof(dto));

        return new Knowledge
        {
            Id = dto.Id,
            DisplayName = dto.DisplayName,
            Description = dto.Description,
            InvestigationContext = dto.InvestigationContext,
            UnlocksInvestigationIntros = dto.UnlocksInvestigationIntros ?? new List<string>(),
            UnlocksInvestigationGoals = dto.UnlocksInvestigationGoals ?? new List<string>()
        };
    }
}
