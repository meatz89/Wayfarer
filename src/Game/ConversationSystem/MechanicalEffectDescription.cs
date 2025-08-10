using System;

/// <summary>
/// Strongly typed description of a mechanical effect for UI display
/// </summary>
public class MechanicalEffectDescription
{
    public string Text { get; set; }
    public EffectCategory Category { get; set; }
    
    // Strongly typed properties for specific effect types
    public ConnectionType? TokenType { get; set; }
    public int? TokenAmount { get; set; }
    public int? TimeMinutes { get; set; }
    public int? LetterPosition { get; set; }
    public string LetterId { get; set; }
    public string NpcId { get; set; }
    public string LocationId { get; set; }
    public string RouteName { get; set; }
    public bool IsObligationBinding { get; set; }
    public bool IsInformationRevealed { get; set; }
}

public enum EffectCategory
{
    TokenGain,
    TokenSpend,
    TimePassage,
    LetterReorder,
    LetterSwap,
    LetterRemove,
    LetterAdd,
    DeadlineExtend,
    InformationGain,
    InformationReveal,
    RouteUnlock,
    LocationUnlock,
    NpcUnlock,
    ObligationCreate,
    StateChange,
    NegotiationOpen,
    InterfaceAction
}