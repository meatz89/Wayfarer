/// <summary>
/// Generates narrative text for Mental tactical system actions
/// Parallel to ConversationNarrativeService in Social system
/// </summary>
public class MentalNarrativeService
{
    public MentalNarrativeService() { }

    public string GenerateActionNarrative(CardInstance card, MentalSession session)
    {
        return card.MentalCardTemplate?.Description ?? "You investigate further.";
    }
}
