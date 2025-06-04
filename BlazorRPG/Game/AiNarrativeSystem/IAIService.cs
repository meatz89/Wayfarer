public interface IAIService
{
    Task<List<EncounterChoice>> RequestChoices(
        string conversationId,
        string systemMessage,
        AIPrompt prompt,
        string initialNarrative,
        int priority);

    string GetProviderName();
}
