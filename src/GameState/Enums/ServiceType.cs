namespace Wayfarer.GameState.Enums
{
    /// <summary>
    /// Type of service being provided in service-based scenes.
    /// Determines which resources are restored, which items are granted, and which buffs apply.
    /// Enables procedural reusability: same mechanical pattern across different service types.
    /// </summary>
    public enum ServiceType
    {
        /// <summary>
        /// Sleeping accommodations at inn or lodging house.
        /// Restores: Health, Stamina, Focus
        /// Grants: room_key
        /// Buff: Focused (organization/preparation)
        /// </summary>
        Lodging,

        /// <summary>
        /// Bathing and grooming services at bathhouse.
        /// Restores: Cleanliness (primary), Health, Stamina
        /// Grants: bathhouse_token
        /// Buff: WellGroomed (appearance/social)
        /// </summary>
        Bathing,

        /// <summary>
        /// Medical treatment at healer or clinic.
        /// Restores: Health (primary), reduced Wounded/Sick states
        /// Grants: treatment_receipt
        /// Buff: Rested (treatment knowledge/health management)
        /// </summary>
        Healing
    }
}
