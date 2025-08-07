using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.ViewModels;

public class ConversationScreenBase : ComponentBase
{
    [Inject] protected GameFacade GameFacade { get; set; }
    [Inject] protected NavigationManager Navigation { get; set; }
    
    [Parameter] public string NpcId { get; set; }
    
    protected ConversationViewModel Model { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await LoadConversation();
    }
    
    protected async Task LoadConversation()
    {
        try
        {
            // First try to get the current pending conversation
            Model = GameFacade.GetCurrentConversation();
            
            // If no pending conversation, create a basic view model
            if (Model == null && !string.IsNullOrEmpty(NpcId))
            {
                Console.WriteLine($"[ConversationScreen] No pending conversation found, using basic model for NPC: {NpcId}");
                Model = GameFacade.GetConversation(NpcId);
            }
            
            if (Model == null)
            {
                Console.WriteLine($"[ConversationScreen] ERROR: Could not load conversation model");
            }
            else
            {
                Console.WriteLine($"[ConversationScreen] Loaded conversation with NPC: {Model.NpcId}, Text: {Model.CurrentText?.Substring(0, Math.Min(50, Model.CurrentText?.Length ?? 0))}...");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading conversation: {ex.Message}");
        }
    }
    
    protected async Task SelectChoice(string choiceId)
    {
        try
        {
            // Process choice through GameFacade
            // This will update the conversation state
            await LoadConversation();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error selecting choice: {ex.Message}");
        }
    }
}