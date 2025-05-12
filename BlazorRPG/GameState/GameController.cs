public class GameController
{
    private readonly OllamaService ollamaService;

    public GameController(NarrativeLogManager logManager, OllamaService ollamaService)
    {
        this.ollamaService = ollamaService;
        string gameInstanceId = Guid.NewGuid().ToString();
    }

    public async Task<NpcCharacter> GenerateNewNpcAsync()
    {
        // Create a console watcher
        IResponseStreamWatcher watcher = new ConsoleResponseWatcher();

        // Generate character with streaming
        NpcCharacter npc = await ollamaService.GenerateCharacterAsync(
            archetype: "merchant",
            region: "coastal town",
            gender: "male",
            minAge: 40,
            maxAge: 60,
            additionalTraits: "seafaring experience, shrewd negotiator",
            watcher: watcher
        );

        return npc;
    }
}