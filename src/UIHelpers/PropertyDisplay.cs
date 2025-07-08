namespace Wayfarer.UIHelpers;

public class PropertyDisplay
{
    public PropertyDisplay()
    {

    }

    public PropertyDisplay(string v1, string v2, string v3, string v4, string v5)
    {
        Text = v1;
        Icon = v2;
        TooltipText = v3;
        CssClass = v4;
        TagName = v5;
    }

    public string Text { get; set; }
    public string Icon { get; set; }
    public string TooltipText { get; set; }
    public string CssClass { get; set; }
    public string TagName { get; set; }
}