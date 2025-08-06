using Microsoft.AspNetCore.Components;

public class AttentionDisplayBase : ComponentBase
{
    [Parameter] public int CurrentAttention { get; set; }
    [Parameter] public int MaxAttention { get; set; } = 3;
    [Parameter] public string Narrative { get; set; }
}