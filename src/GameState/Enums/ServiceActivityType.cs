
/// <summary>
/// Categorical property: Type of service activity offered
///
/// Used by DependentResourceCatalog to procedurally generate dependent locations
/// and access items with activity-appropriate properties and names.
///
/// Enables reusable scene archetypes to work across multiple service types:
/// - Same scene structure (negotiate → access → execute → depart)
/// - Different generated resources (room+key, bath+token, yard+pass)
/// - Appropriate location properties per activity
///
/// CATALOGUE PATTERN: Used at PARSE TIME ONLY by DependentResourceCatalog.
/// </summary>
public enum ServiceActivityType
{
    Lodging,    // Sleeping accommodations (private_room + room_key)
    Bathing,    // Bathing facilities (bath_chamber + bath_token)
    Training,   // Physical training grounds (training_yard + training_pass)
    Healing,    // Medical treatment facilities (treatment_room + treatment_receipt)
    Crafting,   // Workshop access (workshop + workshop_permit)
    Studying    // Study/library access (study_room + library_pass)
}
