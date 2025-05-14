public static class NarrativeServiceUtils
{
    public static PromptManager CreatePromptManager(IConfiguration configuration)
    {
        return new PromptManager(configuration);
    }
}