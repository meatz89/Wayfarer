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

// DEPRECATED FIELDS REMOVED (0% frequency in JSON):
// - Type → hardcoded "Physical", never in JSON, deleted
// - ExertionCost → calculated by catalog, deleted
// - Discipline → always defaulted to "Combat", never in JSON, deleted
// - RiskLevel → always defaulted to "Cautious", never in JSON, deleted
// - ExertionLevel → always defaulted to "Light", never in JSON, deleted
// - MethodType → always defaulted to "Direct", never in JSON, deleted
// - Danger → entire object never in JSON, deleted
}

/// <summary>
/// Requirements nested object - only stats dictionary appears in JSON
/// </summary>
public class PhysicalCardRequirementsDTO
{
// REQUIRED: Stats dictionary (always present, may be empty)
public Dictionary<string, int> Stats { get; set; } = new Dictionary<string, int>();

// DEPRECATED FIELDS REMOVED (0% frequency):
// - EquipmentCategory → never in JSON, deleted
// - Discoveries → never in JSON, deleted
// - MinStamina → never in JSON (costs.stamina in JSON is what you PAY, not requirement), deleted
// - MinHealth → never in JSON (costs.health in JSON is what you PAY, not requirement), deleted
}

/// <summary>
/// Effects nested object - progress and danger appear in JSON
/// </summary>
public class PhysicalCardEffectsDTO
{
// REQUIRED: Progress and Danger (100% frequency)
public int Progress { get; set; } = 0;
public int Danger { get; set; } = 0;

// DEPRECATED FIELDS REMOVED (0% frequency):
// - Discoveries → never in JSON, deleted
}

// DELETED: PhysicalCardDangerDTO - entire object never appears in JSON (0% frequency)
