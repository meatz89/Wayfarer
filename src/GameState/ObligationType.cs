/// <summary>
/// Type of obligation obligation - determines pressure and mechanics
/// Note: Different from BindingObligation.ObligationType (social promises)
/// This enum describes obligation discovery and assignment patterns
/// </summary>
public enum ObligationObligationType
{
/// <summary>
/// Player discovered this obligation through exploration
/// No patron, no deadline, pure freedom
/// Example: Finding the mill mystery by exploring
/// </summary>
SelfDiscovered,

/// <summary>
/// NPC commissioned this obligation with deadline and expectations
/// Has patron NPC, deadline segment, relationship consequences
/// Example: Elena hiring player to deliver package
/// </summary>
NPCCommissioned
}
