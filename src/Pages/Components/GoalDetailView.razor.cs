using Microsoft.AspNetCore.Components;
using System;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// Goal Detail View - Shows full goal information before commitment decision.
    /// </summary>
    public class GoalDetailViewBase : ComponentBase
    {
        [Parameter] public GoalDetailViewModel SelectedGoal { get; set; }

        [Parameter] public EventCallback<Goal> OnCommitToGoal { get; set; }
        [Parameter] public EventCallback OnNavigateBack { get; set; }
    }

    public class GoalDetailViewModel
    {
        public Goal Goal { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TacticalSystemType SystemType { get; set; }
        public string SystemTypeLowercase { get; set; }
        public string Difficulty { get; set; }
        public bool HasCosts { get; set; }
        public int FocusCost { get; set; }
        public int StaminaCost { get; set; }
    }
}
