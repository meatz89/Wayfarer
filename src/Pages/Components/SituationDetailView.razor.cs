using Microsoft.AspNetCore.Components;

/// <summary>
/// Situation Detail View - Shows full situation information before commitment decision.
/// </summary>
public class SituationDetailViewBase : ComponentBase
    {
        [Parameter] public SituationDetailViewModel SelectedSituation { get; set; }

        [Parameter] public EventCallback<Situation> OnCommitToSituation { get; set; }
        [Parameter] public EventCallback OnNavigateBack { get; set; }
    }

    public class SituationDetailViewModel
    {
        public Situation Situation { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TacticalSystemType SystemType { get; set; }
        public string SystemTypeLowercase { get; set; }
        public string Difficulty { get; set; }
        public bool HasCosts { get; set; }
        public int FocusCost { get; set; }
        public int StaminaCost { get; set; }
    }
