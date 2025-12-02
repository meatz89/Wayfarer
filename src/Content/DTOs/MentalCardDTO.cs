/// <summary>
/// DTO for Mental Card
/// Field optionality contract documented in field-optionality-contract.md
/// </summary>
public class MentalCardDTO
{
    // ========== REQUIRED FIELDS (100% frequency in JSON) ==========
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Depth { get; set; }
    public string BoundStat { get; set; }
    public string Method { get; set; }

    // CRITICAL FIX: JSON uses "clueType", not "Category"!
    // MentalCategory: Physical/Testimonial/Deductive/Intuitive
    public string ClueType { get; set; }

    // ========== REQUIRED NESTED OBJECTS (100% frequency) ==========
    // Costs nested object (time, coins) - health/stamina always 0, vestigial
    // Requirements nested object (stats dictionary)
    // Effects nested object (progress, exposure)
    public MentalCardRequirementsDTO Requirements { get; set; }
    public MentalCardEffectsDTO Effects { get; set; }
}

/// <summary>
/// Requirements nested object - only stats list appears in JSON
/// DOMAIN COLLECTION PRINCIPLE: List of objects instead of Dictionary
/// </summary>
public class MentalCardRequirementsDTO
{
    // REQUIRED: Stats list (always present, may be empty)
    public List<StatThresholdDTO> Stats { get; set; } = new List<StatThresholdDTO>();
}

/// <summary>
/// Effects nested object - progress and exposure appear in JSON
/// </summary>
public class MentalCardEffectsDTO
{
    // REQUIRED: Progress and Exposure (100% frequency)
    public int Progress { get; set; } = 0;
    public int Exposure { get; set; } = 0;
}
