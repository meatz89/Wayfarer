/// <summary>
/// DTO for Physical Card
/// Field optionality contract documented in field-optionality-contract.md
/// </summary>
public class PhysicalCardDTO
{
    // ========== REQUIRED FIELDS (100% frequency in JSON) ==========
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Depth { get; set; }
    public string BoundStat { get; set; }
    public string Approach { get; set; }

    // CRITICAL FIX: JSON uses "techniqueType", not "Category"!
    // PhysicalCategory: Strength/Agility/Technique/Endurance
    public string TechniqueType { get; set; }

    // ========== REQUIRED NESTED OBJECTS (100% frequency) ==========
    public PhysicalCardRequirementsDTO Requirements { get; set; }
    public PhysicalCardEffectsDTO Effects { get; set; }
}

/// <summary>
/// Requirements nested object - only stats list appears in JSON
/// DOMAIN COLLECTION PRINCIPLE: List of objects instead of Dictionary
/// </summary>
public class PhysicalCardRequirementsDTO
{
    // REQUIRED: Stats list (always present, may be empty)
    public List<StatThresholdDTO> Stats { get; set; } = new List<StatThresholdDTO>();
}

/// <summary>
/// Effects nested object - progress and danger appear in JSON
/// </summary>
public class PhysicalCardEffectsDTO
{
    // REQUIRED: Progress and Danger (100% frequency)
    public int Progress { get; set; } = 0;
    public int Danger { get; set; } = 0;
}
