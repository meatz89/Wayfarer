public class NarrativeActionContext
{
    public BasicActionTypes ActionType { get; set; }
    public TimeSlots TimeSlot { get; set; }
    public LocationTypes LocationType { get; set; }
    public LocationNames LocationName { get; set; }
    public LocationSpotNames LocationSpot { get; set; }

    // Character Content
    public CharacterNames CharacterName { get; set; }
    public CharacterRoles Role { get; set; }
    public RelationshipStatus Relationship { get; set; }
    public NarrativeStateValues CurrentValues { get; internal set; }
}

public enum ChoiceCategories
{
    // Physical Patterns
    DirectAction,      // Immediate physical response
    CarefulMovement,   // Controlled physical action
    SkillfulTechnique, // Expertise-based movement

    // Social Patterns
    PersonalEngagement,    // One-on-one interaction
    GroupManagement,       // Managing multiple people
    StatusLeverage,        // Using position/authority

    // Strategic Patterns
    ResourceOptimization,  // Managing limited resources
    SpaceControl,         // Managing physical space
    TimingManipulation,   // Using time advantages

    // Recovery Patterns
    ErrorRecovery,        // Fixing mistakes
    TensionDiffusion,     // Calming situations
    OpportunityCreation   // Turning problems to advantages
}

public enum StageProgressions
{
    // Setup Types
    InitialAssessment,    // First look at situation
    OpportunitySpotting,  // Finding advantages
    ResourceGATHERing,    // Collecting needed items

    // Challenge Types
    ExternalComplication, // Outside problem appears
    ResourcePressure,     // Limited resources
    TimeConstraint,       // Time running out
    SocialPressure,       // People causing issues

    // Resolution Types
    ImmediateResolution,  // Quick finish
    GradualImprovement,   // Building to solution
    SituationTransform    // Change the scenario
}