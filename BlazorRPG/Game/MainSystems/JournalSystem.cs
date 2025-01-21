
public class JournalSystem
{
    public string Background { get; set; }
    public string InitialSituation { get; set; }
    public List<string> JourneyEntries{ get; set; } = new();
    public EncounterHistory LastEncounter { get; set; }
    public List<EncounterHistory> EncounterHistory { get; set; } = new();
    private const string NewLine = "\r\n";

    public JournalSystem(GameContentProvider gameContentProvider)
    {
        Background = gameContentProvider.GetBackground;
        InitialSituation = gameContentProvider.GetInitialSituation;
    }

    public List<string> GetDescriptionForCurrentEncounter()
    {
        List<string> descriptions = GetInitialEncounterDescriptions();

        EncounterHistory encounter = LastEncounter;
        string description = InitialSituation + NewLine + NewLine + encounter.TaskToSolve;
        AddToList(descriptions, description);

        foreach (Narrative choice in encounter.Narratives)
        {
            AddToList(descriptions, choice.Text);
        }

        AddToList(descriptions, encounter.ResultSituation);
        return descriptions;
    }

    private List<string> GetInitialEncounterDescriptions()
    {
        List<string> descriptions = new List<string>();
        string description = Background + NewLine + InitialSituation;
        AddToList(descriptions, description);

        foreach (string journeyEntry in JourneyEntries)
        {
            AddToList(descriptions, journeyEntry);
        }

        return descriptions;
    }

    public void WriteJourneyEntry(string narrative)
    {
        JourneyEntries.Add(narrative);
    }

    public void StartEncounter(string taskToSolve, string initialSituation)
    {
        LastEncounter = new EncounterHistory()
        {
            TaskToSolve = taskToSolve,
            InitialSituation = initialSituation,
        };
    }


    public void EndEncounter(string resultSituation)
    {
        LastEncounter.ResultSituation = resultSituation;

        EncounterHistory.Add(LastEncounter);
        LastEncounter = null;
    }

    private void AddToList(List<string> list, string description)
    {
        if (!string.IsNullOrWhiteSpace(description)) list.Add(description);
    }

    public void NoteNewEncounterNarrative(string narrative)
    {
        Narrative item = new Narrative() { Role = Roles.user, Text = narrative };
        LastEncounter.Narratives.Add(item);
    }

    internal void NoteNewEncounterAssistantNarrative(string assistantNarrative)
    {
        Narrative item = new Narrative() { Role = Roles.assistant, Text = assistantNarrative };
        LastEncounter.Narratives.Add(item);
    }
}
