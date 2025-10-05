/// <summary>
/// Generates narrative text for Physical tactical system actions
/// Parallel to ConversationNarrativeService in Social system
/// </summary>
public class PhysicalNarrativeService
{
    public PhysicalNarrativeService() { }

    public string GenerateActionNarrative(CardInstance card, PhysicalSession session)
    {
        return card.PhysicalCardTemplate?.Description ?? "You face the challenge.";
    }
}
