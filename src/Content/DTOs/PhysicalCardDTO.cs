using System.Collections.Generic;

/// <summary>
/// DTO for Physical Card (parallel to MentalCardDTO)
/// </summary>
public class PhysicalCardDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public string Type { get; set; } = "Physical";
    public int Depth { get; set; }
    public string BoundStat { get; set; }

    // Physical-specific properties
    public int ExertionCost { get; set; } = 0;
    public string Approach { get; set; } = "Standard";
    public string Category { get; set; }  // PhysicalCategory: Aggressive/Defensive/Tactical/Evasive/Endurance
    public string Discipline { get; set; } = "Combat";  // PhysicalDiscipline: Combat/Athletics/Finesse/Endurance/Strength

    // Universal card properties (for catalog-based cost calculation)
    public string RiskLevel { get; set; } = "Cautious";
    public string ExertionLevel { get; set; } = "Light";
    public string MethodType { get; set; } = "Direct";

    // Requirements/effects
    public PhysicalCardRequirementsDTO Requirements { get; set; }
    public PhysicalCardEffectsDTO Effects { get; set; }
    public PhysicalCardDangerDTO Danger { get; set; }
}

public class PhysicalCardRequirementsDTO
{
    public string EquipmentCategory { get; set; }  // Categorical equipment requirement (None/Climbing/Mechanical/etc.)
    public List<string> Equipment { get; set; } = new List<string>();  // Legacy - use EquipmentCategory instead
    public List<string> Knowledge { get; set; } = new List<string>();
    public List<string> Discoveries { get; set; } = new List<string>();
    public Dictionary<string, int> Stats { get; set; } = new Dictionary<string, int>();
    public int MinStamina { get; set; } = 0;
    public int MinHealth { get; set; } = 0;
}

public class PhysicalCardEffectsDTO
{
    public int Progress { get; set; } = 0;
    public int Danger { get; set; } = 0;
    public List<string> Discoveries { get; set; } = new List<string>();
}

public class PhysicalCardDangerDTO
{
    public string Type { get; set; }
    public int Probability { get; set; }
    public string Severity { get; set; }
    public DangerEffectDTO Effect { get; set; }
}
