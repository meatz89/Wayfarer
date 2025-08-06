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
            Model = GameFacade.GetConversation(NpcId);
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