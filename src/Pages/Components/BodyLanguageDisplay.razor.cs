using Microsoft.AspNetCore.Components;

public class BodyLanguageDisplayBase : ComponentBase
{
    [Parameter] public string Description { get; set; }
    [Parameter] public string NpcName { get; set; }
}