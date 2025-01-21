public class JournalSystem
{
    public string Background { get; set; }
    public string InitialSituation { get; set; }
    public EncounterHistory LastEncounter { get; set; }
    public List<EncounterHistory> EncounterHistory { get; set; }
    private const string NewLine = "\r\n";

    public JournalSystem(GameContentProvider gameContentProvider)
    {
        Background = gameContentProvider.GetBackground;
        InitialSituation = gameContentProvider.GetInitialSituation;
    }


    public void StartEncounter(string taskToSolve, string initialSituation)
    {
        LastEncounter = new EncounterHistory()
        {
            TaskToSolve = taskToSolve,
            InitialSituation = initialSituation,
        };
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

    public void EndEncounter(string resultSituation)
    {
        LastEncounter.ResultSituation = resultSituation;

        EncounterHistory.Add(LastEncounter);
        LastEncounter = null;
    }


    private List<string> GetInitialEncounterDescriptions()
    {
        List<string> descriptions = new List<string>();
        string description = Background + NewLine + InitialSituation;
        AddToList(descriptions, description);
        return descriptions;
    }

    public List<string> GetDescriptionForCurrentEncounter()
    {
        List<string> descriptions = GetInitialEncounterDescriptions();

        EncounterHistory encounter = LastEncounter;
        string description = InitialSituation + NewLine + encounter.TaskToSolve;
        AddToList(descriptions, description);

        foreach (var choice in encounter.Narratives)
        {
            AddToList(descriptions, choice.Text);
        }

        AddToList(descriptions, encounter.ResultSituation);

        return descriptions;
    }

    private void AddToList(List<string> list, string description)
    {
        if (!string.IsNullOrWhiteSpace(description)) list.Add(description);
    }

}
