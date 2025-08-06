using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class LiteraryConversationScreenBase : ComponentBase
{
    [Inject] protected GameFacade GameFacade { get; set; }
    [Inject] protected NavigationManager Navigation { get; set; }
    
    protected ConversationViewModel Conversation { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await LoadConversation();
    }
    
    protected async Task LoadConversation()
    {
        try
        {
            Conversation = GameFacade.GetCurrentConversation();
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
            GameFacade.ProcessConversationChoice(choiceId);
            await LoadConversation();
            
            // Check if conversation is complete
            if (Conversation?.IsComplete == true)
            {
                Navigation.NavigateTo("/");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error selecting choice: {ex.Message}");
        }
    }
    
    protected async Task TradeRumor(string rumorId)
    {
        try
        {
            // Trade rumor through GameFacade (method would need to be added)
            // await GameFacade.TradeRumor(rumorId);
            await LoadConversation();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error trading rumor: {ex.Message}");
        }
    }
    
    protected string FormatTimeUntilDeadline(int minutes)
    {
        if (minutes < 60)
        {
            return $"{minutes} minutes";
        }
        else if (minutes < 1440) // Less than a day
        {
            int hours = minutes / 60;
            return $"{hours} hour{(hours != 1 ? "s" : "")}";
        }
        else
        {
            int days = minutes / 1440;
            return $"{days} day{(days != 1 ? "s" : "")}";
        }
    }
}