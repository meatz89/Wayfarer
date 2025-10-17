using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// NPC Detail View - Shows one NPC's full details, tokens, and available actions.
    /// </summary>
    public class NPCDetailViewBase : ComponentBase
    {
        [Parameter] public NPCDetailViewModel SelectedNPC { get; set; }
        [Parameter] public List<GoalViewModel> SocialGoals { get; set; } = new();
        [Parameter] public List<ActiveGoalViewModel> NPCActiveGoals { get; set; } = new();
        [Parameter] public bool HasExchangeCards { get; set; }
        [Parameter] public string DoubtDisplay { get; set; }

        [Parameter] public EventCallback<string> OnNavigateToGoal { get; set; }
        [Parameter] public EventCallback<(string npcId, string goalId)> OnStartConversationWithRequest { get; set; }
        [Parameter] public EventCallback<string> OnStartExchange { get; set; }
        [Parameter] public EventCallback OnNavigateBack { get; set; }
    }

    public class NPCDetailViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string PersonalityDescription { get; set; }
        public string ConnectionState { get; set; }
        public string StateClass { get; set; }
        public string Description { get; set; }
        public List<TokenViewModel> Tokens { get; set; } = new();
    }

    public class TokenViewModel
    {
        public string Type { get; set; }
        public int Count { get; set; }
        public string Effect { get; set; }
    }

    public class GoalViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Difficulty { get; set; }
        public bool IsIntroAction { get; set; }
        public string InvestigationId { get; set; }
    }

    public class ActiveGoalViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
