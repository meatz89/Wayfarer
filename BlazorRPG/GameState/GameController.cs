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
        // Generate a villager between 25-40 years old
        NpcCharacter npc = await ollamaService.GenerateCharacterAsync(
            archetype: "villager",
            region: "the northern farmlands",
            gender: "", // empty for any
            minAge: 25,
            maxAge: 40,
            additionalTraits: "knows local herbs, distrustful of outsiders"
        );

        // Save the character to a file
        string filePath = Path.Combine("GameData", "Characters", $"{npc.Name.Replace(" ", "_")}.json");
        //await _ollamaService.SaveCharacterToFileAsync(npc, filePath);

        return npc;
    }
}