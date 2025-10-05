using System.Collections.Generic;

/// <summary>
/// DTO for Mental Card (parallel to ConversationCardDTO)
/// </summary>
public class MentalCardDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public string Type { get; set; } = "Mental";
    public int Depth { get; set; }
    public string BoundStat { get; set; }

    // Mental-specific properties
    public int AttentionCost { get; set; } = 0;
    public string Method { get; set; } = "Standard";
    public string Category { get; set; }  // MentalCategory: Analytical/Physical/Observational/Social/Synthesis

    // Universal card properties (for catalog-based cost calculation)
    public string RiskLevel { get; set; } = "Cautious";
    public string ExertionLevel { get; set; } = "Light";
    public string MethodType { get; set; } = "Direct";

    // Requirements/effects
    public MentalCardRequirementsDTO Requirements { get; set; }
    public MentalCardEffectsDTO Effects { get; set; }
    public MentalCardDangerDTO Danger { get; set; }
}

public class MentalCardRequirementsDTO
{
    public string EquipmentCategory { get; set; }  // Categorical equipment requirement (None/Climbing/Mechanical/etc.)
    public List<string> Equipment { get; set; } = new List<string>();  // Legacy - use EquipmentCategory instead
    public List<string> Knowledge { get; set; } = new List<string>();
    public List<string> Discoveries { get; set; } = new List<string>();
    public Dictionary<string, int> Stats { get; set; } = new Dictionary<string, int>();
    public int MinStamina { get; set; } = 0;
    public int MinHealth { get; set; } = 0;
}

public class MentalCardEffectsDTO
{
    public int Progress { get; set; } = 0;
    public int Exposure { get; set; } = 0;
    public List<string> Discoveries { get; set; } = new List<string>();
}

public class MentalCardDangerDTO
{
    public string Type { get; set; }
    public int Probability { get; set; }
    public string Severity { get; set; }
    public DangerEffectDTO Effect { get; set; }
}
