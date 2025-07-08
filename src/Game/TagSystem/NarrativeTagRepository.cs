/// <summary>
/// Repository of narrative tags reflecting general conditions in a medieval traveler story.
/// The tags are designed to be generic, so they can be applied in any setting regardless of location.
/// Each tag adjusts the focus requirement (Relationship, Information, Physical, Environment, Resource)
/// based on overarching narrative or atmospheric conditions.
/// </summary>
public class NarrativeTagRepository
{
    // -------------------------------
    // RELATIONSHIP FOCUS TAGS
    // -------------------------------

    /// <summary>
    /// "Affable Manner" conveys a warm, welcoming ambiance that makes social interactions easier.
    /// </summary>
    public static NarrativeTag AffableManner = new NarrativeTag(
        "Affable Manner",
        -1  // Reduces the required Relationship focus.
    );

    /// <summary>
    /// "Strained Interaction" reflects tension or mistrust among companions, making social exchanges more demanding.
    /// </summary>
    public static NarrativeTag StrainedInteraction = new NarrativeTag(
        "Strained Interaction",
        +1  // Increases the required Relationship focus.
    );

    // -------------------------------
    // INFORMATION FOCUS TAGS
    // -------------------------------

    /// <summary>
    /// "Lucid Concentration" indicates a clear state of mind that significantly eases the process of gathering information.
    /// </summary>
    public static NarrativeTag LucidConcentration = new NarrativeTag(
        "Lucid Concentration",
        -2  // Greatly reduces the Information focus needed.
    );

    /// <summary>
    /// "Distracting Commotion" suggests a noisy or unsettling atmosphere that disrupts mental clarity.
    /// </summary>
    public static NarrativeTag DistractingCommotion = new NarrativeTag(
        "Distracting Commotion",
        +1  // Increases the required Information focus.
    );

    // -------------------------------
    // PHYSICAL FOCUS TAGS
    // -------------------------------

    /// <summary>
    /// "Fluid Movement" represents conditions where ease and coordination are enhanced, facilitating physical actions.
    /// </summary>
    public static NarrativeTag FluidMovement = new NarrativeTag(
        "Fluid Movement",
        -1  // Lowers the demand for Physical focus.
    );

    /// <summary>
    /// "Unsteady Conditions" denotes a state where physical coordination suffers, making actions more challenging.
    /// </summary>
    public static NarrativeTag UnsteadyConditions = new NarrativeTag(
        "Unsteady Conditions",
        +1  // Raises the required Physical focus.
    );

    // -------------------------------
    // ENVIRONMENT FOCUS TAGS
    // -------------------------------

    /// <summary>
    /// "Harmonious Order" implies that the setting is clear and well-organized, helping to ease environmental awareness.
    /// </summary>
    public static NarrativeTag HarmoniousOrder = new NarrativeTag(
        "Harmonious Order",
        -1  // Decreases the Environmental focus needed.
    );

    /// <summary>
    /// "Disordered Ambience" conveys a scenario of chaos or confusion that complicates the interpretation of one’s surroundings.
    /// </summary>
    public static NarrativeTag DisorderedAmbience = new NarrativeTag(
        "Disordered Ambience",
        +2  // Significantly increases the required Environmental focus.
    );

    // -------------------------------
    // RESOURCE FOCUS TAGS
    // -------------------------------

    /// <summary>
    /// "Plentiful Provisions" indicates that resources are abundant, making management and trade easier.
    /// </summary>
    public static NarrativeTag PlentifulProvisions = new NarrativeTag(
        "Plentiful Provisions",
        -1  // Reduces the required Resource focus.
    );

    /// <summary>
    /// "Scarce Assets" suggests that resources are limited, thus heightening the challenge of managing them.
    /// </summary>
    public static NarrativeTag ScarceAssets = new NarrativeTag(
        "Scarce Assets",
        +1  // Increases the required Resource focus.
    );
}