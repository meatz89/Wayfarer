using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.Services;
using Wayfarer.Models;

namespace Wayfarer.Pages.Components;

public partial class NPCActionsView : ComponentBase
{
    [Inject] private GameFacade GameFacade { get; set; }
    [Inject] private ConversationRepository ConversationRepository { get; set; }
    [Inject] private ITimeManager TimeManager { get; set; }
    [Inject] private NPCRepository NPCRepository { get; set; }
    [Inject] private SpecialLetterGenerationService SpecialLetterService { get; set; }
    [Inject] private ConnectionTokenManager TokenManager { get; set; }
    [Inject] private GameWorld GameWorld { get; set; }
    [Inject] private TimeImpactCalculator TimeCalculator { get; set; }

    [Parameter] public NPC SelectedNPC { get; set; }
    [Parameter] public EventCallback OnActionExecuted { get; set; }

    private TimeBlocks CurrentTime => TimeManager.GetCurrentTimeBlock();

    private List<ActionOptionViewModel> GetActionsForNPC()
    {
        if (SelectedNPC == null) return new List<ActionOptionViewModel>();

        LocationActionsViewModel allActions = GameFacade.GetLocationActions();
        Dictionary<string, List<ActionOptionViewModel>> npcActions = GroupActionsByNPC(allActions);

        return npcActions.ContainsKey(SelectedNPC.ID)
            ? npcActions[SelectedNPC.ID]
            : new List<ActionOptionViewModel>();
    }

    private Dictionary<string, List<ActionOptionViewModel>> GroupActionsByNPC(LocationActionsViewModel viewModel)
    {
        Dictionary<string, List<ActionOptionViewModel>> npcGroups = new Dictionary<string, List<ActionOptionViewModel>>();

        foreach (ActionGroupViewModel group in viewModel.ActionGroups)
        {
            foreach (ActionOptionViewModel action in group.Actions)
            {
                // Match actions to the selected NPC by name
                if (!string.IsNullOrEmpty(action.NPCName) && action.NPCName == SelectedNPC?.Name)
                {
                    string npcId = SelectedNPC.ID;
                    if (!npcGroups.ContainsKey(npcId))
                    {
                        npcGroups[npcId] = new List<ActionOptionViewModel>();
                    }
                    npcGroups[npcId].Add(action);
                }
            }
        }

        return npcGroups;
    }

    private bool CanAffordAction(ActionOptionViewModel action)
    {
        return action.HasEnoughTime && action.HasEnoughStamina && action.HasEnoughCoins && !action.IsServiceClosed;
    }

    private async Task ExecuteAction(string actionId)
    {
        bool result = await GameFacade.ExecuteLocationActionAsync(actionId);

        if (result)
        {
            StateHasChanged();

            if (OnActionExecuted.HasDelegate)
            {
                await OnActionExecuted.InvokeAsync();
            }
        }
    }

    private string GetActionVerb(ActionOptionViewModel action)
    {
        // Extract the verb from the action description
        if (action.Description.StartsWith("Talk to"))
            return "Talk";
        if (action.Description.StartsWith("Buy from"))
            return "Buy";
        if (action.Description.StartsWith("Sell to"))
            return "Sell";
        if (action.Description.StartsWith("Train with"))
            return "Train";
        if (action.Description.StartsWith("Work for"))
            return "Work";
        if (action.Description.Contains("letter"))
            return "Get Letter";
        if (action.Description.Contains("special"))
            return "Request Special";

        return action.Description;
    }

    private List<TokenChange> GetTokenChangesForAction(ActionOptionViewModel action, string npcId)
    {
        List<TokenChange> changes = new List<TokenChange>();

        // For talk actions, show potential token gain
        if (action.Id.StartsWith("talk_"))
        {
            // Check if this NPC has a special conversation
            if (ConversationRepository.HasConversation(npcId))
            {
                changes.Add(new TokenChange
                {
                    NpcName = NPCRepository.GetById(npcId)?.Name ?? npcId,
                    TokenType = ConnectionType.Trust,
                    Amount = 1
                });
            }
        }

        // For special letter requests, token costs are shown separately in the UI

        return changes.Any() ? changes : null;
    }

    private async Task RequestSpecialLetter(string npcId, ConnectionType tokenType)
    {
        // Request special letter directly through GameFacade
        bool result = GameFacade.RequestSpecialLetter(npcId, tokenType);

        if (result)
        {
            StateHasChanged();

            if (OnActionExecuted.HasDelegate)
            {
                await OnActionExecuted.InvokeAsync();
            }
        }
    }

    private string GetSpecialLetterIcon(string specialType)
    {
        return specialType switch
        {
            "Introduction Letter" => "✉️",
            "Access Permit" => "🎫",
            "Endorsement Letter" => "📜",
            "Information Letter" => "🔍",
            _ => "📮"
        };
    }

    private string GetTokenIcon(ConnectionType tokenType)
    {
        return tokenType switch
        {
            ConnectionType.Trust => "❤️",
            ConnectionType.Commerce => "💼",
            ConnectionType.Status => "👑",
            ConnectionType.Shadow => "🌙",
            _ => "●"
        };
    }

    private List<TokenChange> GetSpecialLetterTokenCost(string npcId, SpecialLetterOption option)
    {
        return new List<TokenChange>
        {
            new TokenChange
            {
                NpcName = NPCRepository.GetById(npcId)?.Name ?? npcId,
                TokenType = option.TokenType,
                Amount = -option.Cost
            }
        };
    }
}