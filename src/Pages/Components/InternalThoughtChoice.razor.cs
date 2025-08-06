using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

public class InternalThoughtChoiceBase : ComponentBase
{
    [Parameter] public ConversationChoiceViewModel Choice { get; set; }
    [Parameter] public EventCallback OnChoiceSelected { get; set; }
    [Parameter] public int CurrentAttention { get; set; }
    
    protected async Task HandleClick()
    {
        if (Choice?.IsAvailable == true)
        {
            await OnChoiceSelected.InvokeAsync();
        }
    }
}