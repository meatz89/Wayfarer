namespace BlazorRPG.Pages;

public record struct PropertyDisplay(
    string Icon,
    string Text,
    string CssClass,
    string TooltipText = ""
);
