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
    public List<string> Tags { get; set; } = new List<string>();

    // Mental-specific properties
    public int AttentionCost { get; set; } = 0;
    public string Method { get; set; } = "Standard";
    public string Category { get; set; }  // MentalCategory: Analytical/Physical/Observational/Social/Synthesis

    // Universal costs/requirements/effects
    public MentalCardCostsDTO Costs { get; set; }
    public MentalCardRequirementsDTO Requirements { get; set; }
    public MentalCardEffectsDTO Effects { get; set; }
    public MentalCardDangerDTO Danger { get; set; }
}

public class MentalCardCostsDTO
{
    public int Stamina { get; set; } = 0;
    public int Health { get; set; } = 0;
    public int Time { get; set; } = 1;
    public int Coins { get; set; } = 0;
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
