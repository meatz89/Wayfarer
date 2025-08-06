using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;

public class PeripheralAwarenessBase : ComponentBase
{
    [Parameter] public List<string> Observations { get; set; } = new();
}