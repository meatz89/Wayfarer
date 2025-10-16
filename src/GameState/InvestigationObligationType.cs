/// <summary>
/// Type of investigation obligation - determines pressure and mechanics
/// Note: Different from BindingObligation.ObligationType (social promises)
/// This enum describes investigation discovery and assignment patterns
/// </summary>
public enum InvestigationObligationType
{
    /// <summary>
    /// Player discovered this investigation through exploration
    /// No patron, no deadline, pure freedom
    /// Example: Finding the mill mystery by exploring
    /// </summary>
    SelfDiscovered,

    /// <summary>
    /// NPC commissioned this investigation with deadline and expectations
    /// Has patron NPC, deadline segment, relationship consequences
    /// Example: Elena hiring player to deliver package
    /// </summary>
    NPCCommissioned
}
