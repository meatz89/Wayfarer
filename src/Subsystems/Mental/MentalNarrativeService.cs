/// <summary>
/// Generates narrative text for Mental tactical system actions
/// Parallel to ConversationNarrativeService in Social system
/// </summary>
public class MentalNarrativeService
{
    public MentalNarrativeService() { }

    public string GenerateActionNarrative(CardInstance card, MentalSession session)
    {
        if (card.MentalCardTemplate == null)
            throw new InvalidOperationException("CardInstance missing MentalCardTemplate");

        return card.MentalCardTemplate.Description;
    }
}
