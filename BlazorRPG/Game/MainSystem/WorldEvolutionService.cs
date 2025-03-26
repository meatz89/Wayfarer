public class WorldEvolutionService
{
    public NarrativeService narrativeService { get; }

    public WorldEvolutionService(NarrativeService narrativeService)
    {
        this.narrativeService = narrativeService;
    }

    public async Task<WorldEvolutionResponse> ProcessWorldEvolution(NarrativeContext context, WorldEvolutionInput input)
    {
        WorldEvolutionResponse response = await narrativeService.ProcessWorldEvolution(context, input);
        return response;
    }

    public async Task<string> ConsolidateMemory(NarrativeContext context, MemoryConsolidationInput input)
    {
        string response = await narrativeService.ProcessMemoryConsolidation(context, input);
        return response;
    }

}
